#include "platform/pe.h"

// Windows.h 默认把 min/max 定义成宏，会把下面用到的 std::min / std::max 啃掉
#define NOMINMAX
#include <Windows.h>

#include <algorithm>
#include <cstring>
#include <fstream>
#include <stdexcept>

namespace
{
	// 本类按编译目标的位宽解析 PE，只接受 OptionalHeader Magic 与之一致的映像：
	// IMAGE_NT_HEADERS / IMAGE_OPTIONAL_HEADER 本身就是按位宽选择的类型。
#if defined(_WIN64)
	constexpr WORD kExpectedMagic = IMAGE_NT_OPTIONAL_HDR64_MAGIC;
#else
	constexpr WORD kExpectedMagic = IMAGE_NT_OPTIONAL_HDR32_MAGIC;
#endif

	int GetFileSize(std::ifstream &ifs)
	{
		auto const end = ifs.seekg(0, std::ios::end).tellg();
		ifs.seekg(0, std::ios::beg);

		return static_cast<int>(end);
	}

	// 从缓冲区里按值取出一个结构体，并做越界检查。
	// 用 memcpy 而不是直接 reinterpret_cast，是为了避开未对齐访问。
	template <typename T>
	T ReadAt(std::vector<uint8_t> const &buffer, size_t offset, char const *what)
	{
		if (offset > buffer.size() || buffer.size() - offset < sizeof(T)) {
			throw std::runtime_error(std::string("Truncated PE image while reading ") + what);
		}

		T value{};
		std::memcpy(&value, buffer.data() + offset, sizeof(T));
		return value;
	}

	std::string ReadCStr(std::vector<uint8_t> const &buffer, size_t offset, size_t maxLength, char const *what)
	{
		if (offset >= buffer.size()) {
			throw std::runtime_error(std::string("Truncated PE image while reading ") + what);
		}

		auto const *begin = buffer.data() + offset;
		size_t const limit = std::min(maxLength, buffer.size() - offset);
		auto const *end = std::find(begin, begin + limit, '\0');

		return std::string(begin, end);
	}
}

toes::pe::PortableExecutable::PortableExecutable(std::filesystem::path const &filepath)
{
	if (!std::filesystem::exists(filepath)) {
		throw std::filesystem::filesystem_error(
			"File does not exist.", filepath, std::make_error_code(std::errc::no_such_file_or_directory));
	}

	std::ifstream ifs(filepath, std::ios::binary);
	if (!ifs.is_open()) {
		throw std::runtime_error("Open file " + filepath.string() + " failed.");
	}

	int const fileSize = GetFileSize(ifs);
	if (fileSize < static_cast<int>(sizeof(IMAGE_DOS_HEADER))) {
		throw std::runtime_error("File " + filepath.string() + " is too small to be a PE image.");
	}

	m_fileBuffer.resize(static_cast<size_t>(fileSize));
	ifs.read(reinterpret_cast<char *>(m_fileBuffer.data()), fileSize);
	if (ifs.gcount() != fileSize) {
		throw std::runtime_error("Failed to read the whole of " + filepath.string() + ".");
	}

	auto const dosHeader = ReadAt<IMAGE_DOS_HEADER>(m_fileBuffer, 0, "DOS header");
	if (dosHeader.e_magic != IMAGE_DOS_SIGNATURE) {
		throw std::runtime_error(filepath.string() + " is not a DOS .EXE file.");
	}

	m_ntOffset = static_cast<size_t>(dosHeader.e_lfanew);

	auto const ntHeader = ReadAt<IMAGE_NT_HEADERS>(m_fileBuffer, m_ntOffset, "NT header");
	if (ntHeader.Signature != IMAGE_NT_SIGNATURE) {
		throw std::runtime_error(filepath.string() + " is not a PE file.");
	}
	if (ntHeader.OptionalHeader.Magic != kExpectedMagic) {
		throw std::runtime_error(filepath.string() + " is not a " +
			((sizeof(void *) == 8) ? "64-bit" : "32-bit") + " PE image.");
	}
}

unsigned long toes::pe::PortableExecutable::GetTimeDateStamp() const
{
	auto const ntHeader = ReadAt<IMAGE_NT_HEADERS>(m_fileBuffer, m_ntOffset, "NT header");
	return ntHeader.FileHeader.TimeDateStamp;
}

unsigned long toes::pe::PortableExecutable::GetFoa(unsigned long va) const
{
	auto const ntHeader = ReadAt<IMAGE_NT_HEADERS>(m_fileBuffer, m_ntOffset, "NT header");

	// 头部与节表在文件里是连续存放的，这段区域的 RVA 与文件偏移相同
	if (va < ntHeader.OptionalHeader.SizeOfHeaders) {
		return va;
	}

	auto const sectionsOffset = m_ntOffset + sizeof(IMAGE_NT_HEADERS);
	for (WORD i = 0; i < ntHeader.FileHeader.NumberOfSections; ++i) {
		auto const section = ReadAt<IMAGE_SECTION_HEADER>(
			m_fileBuffer, sectionsOffset + static_cast<size_t>(i) * sizeof(IMAGE_SECTION_HEADER), "section header");

		// 有些编译器会把 VirtualSize 留成 0，此时退回用 SizeOfRawData
		unsigned long const span = std::max(section.Misc.VirtualSize, section.SizeOfRawData);
		if (va >= section.VirtualAddress && va - section.VirtualAddress < span) {
			return section.PointerToRawData + (va - section.VirtualAddress);
		}
	}

	throw std::out_of_range("RVA 0x" + std::to_string(va) + " is not covered by any section");
}

std::shared_ptr<toes::pe::ExportTable> toes::pe::PortableExecutable::GetExportTable() const
{
	auto const ntHeader = ReadAt<IMAGE_NT_HEADERS>(m_fileBuffer, m_ntOffset, "NT header");
	auto const directory = ntHeader.OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];

	if (directory.VirtualAddress == 0 || directory.Size < sizeof(IMAGE_EXPORT_DIRECTORY)) {
		throw std::domain_error("image has no export directory");
	}

	auto const exportDirectory = ReadAt<IMAGE_EXPORT_DIRECTORY>(
		m_fileBuffer, GetFoa(directory.VirtualAddress), "export directory");

	auto table = std::make_shared<ExportTable>();
	table->DllName = ReadCStr(m_fileBuffer, GetFoa(exportDirectory.Name), 0x1000, "export DLL name");

	auto const namesOffset = GetFoa(exportDirectory.AddressOfNames);
	auto const ordinalsOffset = GetFoa(exportDirectory.AddressOfNameOrdinals);
	auto const functionsOffset = GetFoa(exportDirectory.AddressOfFunctions);

	table->Entries.reserve(exportDirectory.NumberOfNames);
	for (DWORD i = 0; i < exportDirectory.NumberOfNames; ++i) {
		auto const nameRva = ReadAt<DWORD>(
			m_fileBuffer, namesOffset + static_cast<size_t>(i) * sizeof(DWORD), "export name RVA");
		auto const ordinalIndex = ReadAt<WORD>(
			m_fileBuffer, ordinalsOffset + static_cast<size_t>(i) * sizeof(WORD), "export ordinal index");
		auto const functionRva = ReadAt<DWORD>(
			m_fileBuffer, functionsOffset + static_cast<size_t>(ordinalIndex) * sizeof(DWORD), "export function RVA");

		ExportEntry entry;
		entry.Name = ReadCStr(m_fileBuffer, GetFoa(nameRva), 0x1000, "export name");
		entry.Ordinal = exportDirectory.Base + ordinalIndex;
		entry.Rva = functionRva;

		// 地址落在导出目录区间内的，其实是一条转发器字符串
		if (functionRva >= directory.VirtualAddress && functionRva - directory.VirtualAddress < directory.Size) {
			entry.Forwarder = ReadCStr(m_fileBuffer, GetFoa(functionRva), 0x1000, "export forwarder");
		}

		table->Entries.push_back(std::move(entry));
	}

	return table;
}

