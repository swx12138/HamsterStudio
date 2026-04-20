#pragma once

#include <string_view>
#include <iostream>
#include <algorithm>
#include <filesystem>
#include <format>
#include <random>
#include <mutex>

namespace Win32 {
	#include <Windows.h>
	#include <shellapi.h>
	#include <TlHelp32.h>
	#include <Psapi.h>
}

#pragma warning(push)
#pragma warning(disable: 4267)
#pragma warning(disable: 4244)

namespace util
{
	using Win32::MultiByteToWideChar;
	using Win32::WideCharToMultiByte;
	using Win32::GetUserNameA;
	using Win32::CreateToolhelp32Snapshot;
	using Win32::CloseHandle;
	using Win32::OpenProcess;
	using Win32::TerminateProcess;

	using Win32::TCHAR;
	using Win32::DWORD;
	using Win32::PROCESSENTRY32;
	using Win32::PROCESS_INFORMATION;
	using Win32::HANDLE;
	using Win32::LONG_PTR;
	using Win32::STARTUPINFO;


	std::string_view user_name()
	{
		char sz[0x100];
		DWORD dw = 0x100;
		GetUserNameA(sz, &dw);
		return std::string_view(sz);
	}

	long find_process(std::wstring_view strProcessName)
	{
		PROCESSENTRY32 ps = { 0 };
		memset(&ps, 0, sizeof(PROCESSENTRY32));
		ps.dwSize = sizeof(PROCESSENTRY32);

		PROCESS_INFORMATION pi = { 0 };
		memset(&pi, 0, sizeof(PROCESS_INFORMATION));

		HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (hSnapshot == INVALID_HANDLE_VALUE)
			return -1;
		if (!Process32First(hSnapshot, &ps))
			return -2;
		do
		{
			if (strProcessName == ps.szExeFile)
			{
				CloseHandle(hSnapshot);
				return ps.th32ProcessID;
			}
		} while (Process32Next(hSnapshot, &ps));
		CloseHandle(hSnapshot);
		return -3;
	}

	bool start_process(std::wstring_view strProcessName)
	{
		STARTUPINFO si;
		ZeroMemory(&si, sizeof(si));
		si.cb = sizeof(si);
		si.wShowWindow = true;

		PROCESS_INFORMATION pi;
		ZeroMemory(&pi, sizeof(pi));

		if (!CreateProcess(strProcessName.data(), 0, 0, 0, 0, 0, 0, 0, &si, &pi))
		{
			return false;
		}

		CloseHandle(pi.hProcess);
		CloseHandle(pi.hThread);
		return true;
	}

	bool terminate_process(long pid)
	{
		if (pid >= 0)
		{
			// system(std::format("killtask /pid /f {}", pid).data());

			HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);

			TerminateProcess(hProcess, 0);
			CloseHandle(hProcess);

			return true;
		}
		return false;
	}

	namespace stdfs = std::filesystem;

	constexpr std::vector<std::string_view> ImageExtensions{
		".jpg", ".jpeg", ".jpe",
		".png",
		".bmp", ".dib",
		".pbm", ".pgm", ".ppm",
		".sr", ".ras",
		".tiff", ".tif",
		".exr",
		".jp2"
	};

	bool ImageFilterOpenCV(stdfs::path const& path)
	{
		auto filename = path.string();
		for (auto& ch : filename)
		{
			ch = tolower(ch);
		}

		if (filename.ends_with(".jpg") || filename.ends_with(".jpeg") || filename.ends_with(".jpe") ||
			filename.ends_with(".png") ||
			filename.ends_with(".bmp") || filename.ends_with(".dib") ||
			filename.ends_with(".pbm") || filename.ends_with(".pgm") || filename.ends_with(".ppm") ||
			filename.ends_with(".sr") || filename.ends_with(".ras") ||
			filename.ends_with(".tiff") || filename.ends_with(".tif") ||
			filename.ends_with(".exr") ||
			filename.ends_with(".jp2"))
		{
			return true;
		}

		return false;
	}

	namespace fs
	{
		std::vector<std::string> GetFiles(stdfs::path const& _Path, bool (*filter)(stdfs::path const&), bool recur)
		{
			std::vector<std::string> retList;
			if (recur)
			{
				for (auto const& it : std::filesystem::recursive_directory_iterator{ _Path, std::filesystem::directory_options::skip_permission_denied })
				{
					if (!it.is_directory())
					{
						if (auto path = it.path(); path.has_extension() && filter(path))
						{
							retList.push_back(path.string());
						}
					}
				}
			}
			else
			{
				for (auto const& it : std::filesystem::directory_iterator{ _Path })
				{
					if (!it.is_directory())
					{
						if (auto path = it.path(); path.has_extension() && filter(path))
						{
							retList.push_back(path.string());
						}
					}
				}
			}
			return retList;
		}

		std::filesystem::path local_low()
		{
			return std::filesystem::path(std::format("C:\\Users\\{}\\AppData\\LocalLow", user_name()));
		}

	}

} // namespace util

#pragma warning(pop)
