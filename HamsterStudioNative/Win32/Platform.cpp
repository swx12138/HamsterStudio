#include "Platform.h"

#include <Windows.h>

#include <fstream>

int GetFileSize(std::ifstream &ifs)
{
    auto current = ifs.tellg();

    auto end = ifs.seekg(0, std::ios::end).tellg();
    ifs.seekg(0, std::ios::beg);

    return static_cast<int>(end);
}

Platform::Windows::PortableExecutable::PortableExecutable(std::filesystem::path const &filepath)
{
    if (!std::filesystem::exists(filepath))
    {
        throw std::filesystem::filesystem_error("File does not exist.", filepath, std::make_error_code(std::errc::no_such_file_or_directory));
    }

    std::ifstream ifs(filepath, std::ios::binary);
    if (!ifs.is_open())
    {
        throw std::runtime_error("Open file " + filepath.string() + " failed.");
    }

    int fileszie = GetFileSize(ifs);
    pFileBuffer = new uint8_t[fileszie];
    if (pFileBuffer == nullptr)
    {
        throw std::bad_alloc();
    }

    int n = ifs.read((char *)pFileBuffer, fileszie).gcount();

    // ¶ÁDOSÍ·
    PIMAGE_DOS_HEADER dosHeader = (PIMAGE_DOS_HEADER)pFileBuffer;
    if (dosHeader->e_magic != IMAGE_DOS_SIGNATURE)
    {
        throw std::runtime_error(filepath.string() + " is not a DOS .EXE file.");
    }

    // ¶ÁNTÍ·
    PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)(pFileBuffer + dosHeader->e_lfanew);
    if (ntHeaders->Signature != IMAGE_NT_SIGNATURE)
    {
        throw std::runtime_error(filepath.string() + " is not a PE file.");
    }

    PIMAGE_SECTION_HEADER pIMAGE_SECTION_HEADER = (PIMAGE_SECTION_HEADER)(ntHeaders + 1);
    for (int i = 0; i < ntHeaders->FileHeader.NumberOfSections; i++)
    {
        _SecHeaders.push_back(pIMAGE_SECTION_HEADER + i);
    }

    _DosHeader = dosHeader;
    _NtHeader = ntHeaders;
}

Platform::Windows::PortableExecutable::~PortableExecutable()
{
    if (pFileBuffer != nullptr)
    {
        delete[] pFileBuffer;
    }
}

unsigned long Platform::Windows::PortableExecutable::GetFoa(unsigned long va) const
{
    auto dos = std::any_cast<PIMAGE_DOS_HEADER>(_DosHeader);
    auto nt = std::any_cast<PIMAGE_NT_HEADERS>(_NtHeader);

    auto secs = std::shared_ptr<PIMAGE_SECTION_HEADER>(new PIMAGE_SECTION_HEADER[_SecHeaders.size()]);
    for (int i = 0; i < _SecHeaders.size(); i++)
    {
        i[secs.get()] = std::any_cast<PIMAGE_SECTION_HEADER>(_SecHeaders[i]);
    }
    if (nt->OptionalHeader.Magic == IMAGE_NT_OPTIONAL_HDR64_MAGIC)
    {
    }
    throw std::logic_error("not supported platform");
}

unsigned long Platform::Windows::PortableExecutable::GetTimeDateStamp() const
{
    PIMAGE_NT_HEADERS headers = std::any_cast<PIMAGE_NT_HEADERS>(_NtHeader);
    return headers->FileHeader.TimeDateStamp;
}

std::shared_ptr<Platform::Windows::ExportTable> Platform::Windows::PortableExecutable::GetExportTable() const
{
    PIMAGE_NT_HEADERS headers = std::any_cast<PIMAGE_NT_HEADERS>(_NtHeader);
    IMAGE_DATA_DIRECTORY data_director_export = headers->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
    if (data_director_export.Size < sizeof(IMAGE_EXPORT_DIRECTORY))
    {
        throw std::domain_error("data_director_export.Size != sizeof(IMAGE_EXPORT_DIRECTORY)");
    }

    IMAGE_EXPORT_DIRECTORY export_directory = {0};
    auto foa = GetFoa(data_director_export.VirtualAddress);
    memcpy_s(&export_directory, sizeof(IMAGE_EXPORT_DIRECTORY), pFileBuffer + foa, sizeof(IMAGE_NT_HEADERS));

    return std::shared_ptr<ExportTable>();
}