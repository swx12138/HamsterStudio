#pragma once

#include <cstdint>
#include <filesystem>
#include <memory>
#include <string>
#include <string_view>
#include <vector>

namespace toes::pe
{
	// 一条导出记录。Rva 是相对映像基址的偏移；被转发到其他 DLL 的导出项由
	// Forwarder 指明目标（形如 "NTDLL.RtlAllocateHeap"），此时 Rva 指向转发器字符串。
	struct ExportEntry
	{
		std::string Name;
		unsigned long Ordinal = 0;
		unsigned long Rva = 0;
		std::string Forwarder;
	};

	class ExportTable
	{
	public:
		std::string DllName;
		std::vector<ExportEntry> Entries;

		ExportEntry const *Find(std::string_view name) const
		{
			for (auto const &entry : Entries) {
				if (entry.Name == name) {
					return &entry;
				}
			}
			return nullptr;
		}
	};

	class PortableExecutable
	{
	public:
		explicit PortableExecutable(std::filesystem::path const &filepath);

		// 文件内容由 vector 持有，头信息按需从缓冲区里读，不再缓存裸指针，
		// 因此也不需要手写析构；拷贝仍然禁止，因为复制一份文件内容没有意义。
		PortableExecutable(PortableExecutable const &) = delete;
		PortableExecutable &operator=(PortableExecutable const &) = delete;

		/// <summary>
		/// 把 RVA（相对映像基址的偏移）换算成文件偏移。落在头部区域的地址原样返回。
		/// </summary>
		unsigned long GetFoa(unsigned long va) const;

		/// <summary>
		/// The low 32 bits of the time stamp of the image.
		/// This represents the date and time the image was created by the linker.
		/// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970,
		/// Universal Coordinated Time, according to the system clock.
		/// </summary>
		/// <returns></returns>
		unsigned long GetTimeDateStamp() const;

		/// <summary>
		/// 解析导出表。映像没有导出目录时抛 std::domain_error。
		/// </summary>
		std::shared_ptr<ExportTable> GetExportTable() const;

	private:
		std::vector<uint8_t> m_fileBuffer;
		size_t m_ntOffset = 0;
	};

}
