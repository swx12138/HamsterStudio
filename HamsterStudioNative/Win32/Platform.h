#pragma once

#include <filesystem>
#include <string>
#include <any>

namespace Platform
{
	namespace Windows
	{
		class ExportTable
		{

		};

		class PortableExecutable
		{
		public:
			PortableExecutable(std::filesystem::path const &filepath);

			~PortableExecutable();

			unsigned long GetFoa(unsigned long va) const;

			/// <summary>
			/// The low 32 bits of the time stamp of the image. 
			/// This represents the date and time the image was created by the linker. 
			/// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, 
			/// Universal Coordinated Time, according to the system clock.
			/// </summary>
			/// <returns></returns>
			unsigned long GetTimeDateStamp() const;

			std::shared_ptr<ExportTable> GetExportTable() const;

		private:
			uint8_t *pFileBuffer;
			std::any _DosHeader;
			std::any _NtHeader;
			std::vector<std::any> _SecHeaders;
		};

	}
}
