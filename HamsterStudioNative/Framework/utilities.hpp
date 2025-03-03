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

namespace util
{
	class CmdLineArgs
	{
	public:
		using Args = std::vector<std::string_view>;

		std::string_view operator[](std::string_view key) const
		{
			if (has_key(key))
			{
				return get_value(key);
			}
			throw std::exception(std::format("can't find key:{}", key).data());
		}

		inline bool is_ready() const
		{
			return !args.empty();
		}

		int has_key(std::string_view key) const
		{
			for (auto iter = args.begin(); iter != args.end(); iter++)
			{
				if (_Match_Key(key, *iter))
				{
					return static_cast<int>(iter - args.begin());
				}
			}
			return false;
		}

		std::string_view get_value(std::string_view key) const
		{
			for (auto iter = args.begin(); iter != args.end(); iter++)
			{
				if (_Match_Key(key, *iter))
				{
					if (++iter != args.end())
					{
						return *iter;
					}
					break;
				}
			}
			throw std::exception(std::format("no matched param with \"{}\".", key).data());
		}

		void print_all() const
		{
			for (auto const& arg : args)
			{
				std::cout << arg << std::endl;
			}
		}

		static CmdLineArgs& getInst()
		{
			static CmdLineArgs m_Inst;
			if (!m_Inst.is_ready())
			{
				std::lock_guard<std::mutex> g(alloc_inst);
				if (!m_Inst.is_ready())
				{
					m_Inst = CmdLineArgs{ __argc, __argv };
				}
			}
			return m_Inst;
		}

	private:
		CmdLineArgs() {}

		CmdLineArgs(int argc, char** argv)
		{
			for (int i = 0; i < argc; i++)
			{
				args.push_back(argv[i]);
			}
		}

		bool _Match_Key(std::string_view key, std::string_view val) const
		{
			return ('-' == val[0] && key == val.substr(1)) ||
				('-' == val[1] && key == val.substr(2));
		}

	private:
		static std::mutex alloc_inst;
		Args args;
	};

	std::mutex CmdLineArgs::alloc_inst;

	using Win32::MultiByteToWideChar;
	using Win32::WideCharToMultiByte;
	using Win32::GetUserNameA;
	using Win32::CreateToolhelp32Snapshot;
	using Win32::CloseHandle;
	using Win32::OpenProcess;
	using Win32::TerminateProcess;

	namespace Huawei
	{
		//@brief:UTF8 to string
		std::string utf8_to_string(std::string const& str)
		{
			long long nwLen = MultiByteToWideChar(CP_UTF8, 0, str.c_str(), -1, NULL, 0);

			wchar_t* pwBuf = new wchar_t[nwLen + 1u]; // Be sure to add 1, otherwise there will be a tail
			memset(pwBuf, 0, static_cast<long long>(nwLen * 2u + 2));

			MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), pwBuf, nwLen);

			long long nLen = WideCharToMultiByte(CP_ACP, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

			char* pBuf = new char[nLen + 1u];
			memset(pBuf, 0, nLen + 1u);

			WideCharToMultiByte(CP_ACP, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

			std::string retStr = pBuf;

			delete[] pBuf;
			delete[] pwBuf;

			pBuf = NULL;
			pwBuf = NULL;

			return retStr;
		}

		//@brief:string to UTF8
		std::string string_to_utf8(std::string const& str)
		{
			long long nwLen = MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, NULL, 0);

			wchar_t* pwBuf = new wchar_t[nwLen + 1u]; // Be sure to add 1, otherwise there will be a tail
			ZeroMemory(pwBuf, nwLen * 2 + 2);

			MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), pwBuf, nwLen);

			long long nLen = WideCharToMultiByte(CP_UTF8, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

			char* pBuf = new char[nLen + 1u];
			ZeroMemory(pBuf, nLen + 1);

			WideCharToMultiByte(CP_UTF8, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

			std::string retStr(pBuf);

			delete[] pwBuf;
			delete[] pBuf;

			pwBuf = NULL;
			pBuf = NULL;

			return retStr;
		}
	}

	std::string to_string(std::wstring const& wstr)
	{
		std::string result;
		int len = WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.size(), NULL, 0, NULL, NULL);
		char* buffer = new char[len + 1];
		WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.size(), buffer, len, NULL, NULL);
		buffer[len] = '\0';
		result.append(buffer);
		delete[] buffer;
		return result;
	}

	using Win32::TCHAR;
	using Win32::DWORD;
	using Win32::PROCESSENTRY32;
	using Win32::PROCESS_INFORMATION;
	using Win32::HANDLE;
	using Win32::LONG_PTR;
	using Win32::STARTUPINFO;

	std::wstring to_wstring(std::string const& str)
	{
		std::wstring result;
		int len = MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.size(), NULL, 0);
		TCHAR* buffer = new TCHAR[len + 1];
		MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.size(), buffer, len);
		buffer[len] = '\0';
		result.append(buffer);
		delete[] buffer;
		return result;
	}

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

	using StringList = std::vector<std::string>;

	template <typename _Ty>
	_Ty sum(std::vector<_Ty> const& arr)
	{
		_Ty ans{};
		for (auto const& e : arr)
		{
			ans += e;
		}
		return ans;
	}

	template <typename _Ty>
	std::string join(std::vector<_Ty> const& vec, std::string delim)
	{
		std::ostringstream _Stream;
		_Stream << *vec.cbegin();
		std::for_each(vec.cbegin() + 1, vec.cend(), [=, &_Stream](auto const val)
			{ _Stream << delim << val; });
		return _Stream.str();
	}

	StringList split(std::string const& str, std::string delim)
	{
		std::vector<int> _Tmp;
		auto _Pos = str.find_first_of(delim);
		while (_Pos != std::string::npos)
		{
			_Tmp.push_back(_Pos);
			_Pos = str.find_first_of(delim, _Pos + delim.length());
		}

		std::vector<std::string> _Ans;
		_Ans.emplace_back(str.substr(0, _Tmp[0]));
		(void)std::adjacent_find(_Tmp.cbegin(), _Tmp.cend(), [=, &_Ans](auto a, auto b)
			{ a += delim.length(); _Ans.push_back(str.substr(a, b - a)); return false; });
		_Ans.emplace_back(str.substr(_Tmp[_Tmp.size() - 1] + delim.length()));
		return _Ans;
	}

	StringList split2(std::string const& str, char const delim)
	{
		StringList ret;
		std::stringstream ss(str);
		std::string ea;
		while (std::getline(ss, ea, delim))
		{
			ret.push_back(ea);
		}
		return ret;
	}

	std::string replace(std::string src, std::string const& which, std::string const& toWhat)
	{
		auto pos = src.find(which);
		while (pos != std::string::npos)
		{
			src.erase(pos, which.length());
			src.insert(pos, toWhat);
			pos = src.find(which);
		}
		return src;
	}

	using ByteArray = std::vector<uint8_t>; // std::basic_string<unsigned char>;

	ByteArray operator""_ba(const char* szData, const size_t len)
	{
		ByteArray _Ans;
		for (int i = 0u; i < len; i++)
		{
			_Ans.push_back(szData[i]);
		}
		return _Ans;
	}

	namespace stdfs = std::filesystem;

	std::string AddNumber(std::string num1, std::string num2)
	{
		std::string _Ans;
		auto iter1 = num1.crbegin(), iter2 = num2.crbegin();
		int _Jin = 0;
		auto _Fix_Value = [&](char& value)
		{
			if (value > '9')
			{
				_Jin = (value - '0') / 10;
				value -= 10;
			}
			else
			{
				_Jin = 0;
			}
		};
		while (iter1 != num1.crend() && iter2 != num2.crend())
		{
			char value = *(iter1++) - '0' + *(iter2++) + _Jin;
			_Fix_Value(value);
			_Ans.push_back(value);
		}
		while (iter1 != num1.crend())
		{
			char value = *(iter1++) + _Jin;
			_Fix_Value(value);
			_Ans.push_back(value);
		}
		while (iter2 != num2.crend())
		{
			char value = *(iter2++) + _Jin;
			_Fix_Value(value);
			_Ans.push_back(value);
		}
		if (_Jin != 0)
		{
			_Ans.push_back(_Jin + '0');
		}
		reverse(_Ans.begin(), _Ans.end());
		return _Ans;
	}

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

	std::string password(int _Len = 16)
	{
		constexpr std::string_view templ{ "0123456789.AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz." };

		std::string str;
		str.resize(_Len);
		std::mt19937 gen(std::random_device{}());
		std::uniform_int_distribution<> dist(0, templ.length() - 1);
		for (auto& ch : str)
		{
			ch = templ[dist(gen)];
		}

		return str;
	}

	namespace fs
	{
		StringList GetFiles(stdfs::path const& _Path, bool (*filter)(stdfs::path const&), bool recur)
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

	ByteArray GetBytes(std::string const& str)
	{
		return ByteArray(str.cbegin(), str.cend());
	}

	std::string Hex(ByteArray const& byts, bool upper = false)
	{
		char szBuffer[3];
		std::vector<char> _Ans;
		for (auto byt : byts)
		{
			sprintf_s(szBuffer, 3, upper ? "%2X" : "%2x", byt);
			_Ans.emplace_back(szBuffer[0]);
			_Ans.emplace_back(szBuffer[1]);
		}
		return std::string(_Ans.cbegin(), _Ans.cend());
	}

	ByteArray Dehex(std::string const& str)
	{
		const auto _Dehex = [](char ch)
		{
			if (isdigit(ch))
				return ch - '0';
			else if (isupper(ch))
				return ch - 'A';
			else if (islower(ch))
				return ch - 'a';
			return -1;
		};

		ByteArray _Ans;
		for (auto iter = str.cbegin(); iter != str.cend();)
		{
			auto val = _Dehex(*iter++);
			_Ans.push_back(val * 16 + _Dehex(*iter++));
		}
		return _Ans;
	}

	class Application
	{

	public:
		void Exit(int nCode) const
		{
			return exit(nCode);
		}
	};

	namespace Debug
	{
		void Assert(bool condition, std::string const& message = "")
		{
			if (!condition)
			{
				throw std::exception(message.data());
			}
		}
	}

	class BigInteger {
		using Reference = BigInteger&;
		using Const_Reference = const Reference;

	public:
		BigInteger() {
			_Num = "0";
		}

		BigInteger(std::string const& val)
			: _Num(val) {}

		BigInteger(int val) {
			*this = val;
		}

		void operator=(int val) {
			_Num = std::to_string(val);
		}

		BigInteger operator+(Const_Reference bigNumber) {
			return AddNumber(_Num, bigNumber._Num);
		}

		bool operator==(std::string const& val) {
			return val == _Num;
		}

		static bool isValidNum(std::string const& str) {
			if (str.length() == 0) {
				return false;
			}
			if (str.length() != 1 && str[0] == '0') {
				return false;
			}
			return all_of(str.cbegin(), str.cend(), isdigit);
		}

		operator std::string() const {
			return _Num;
		}

	private:
		std::string _Num;
	};

	std::string keygen(size_t len = 16) {
		constexpr static std::string_view templ = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789-_.";
		//for (auto i = 0; i < 26; i++) {
		//	putchar('a' + i);
		//	putchar('A' + i);
		//}
		//for (auto i = 0; i < 10; i++) {
		//	putchar('0' + i);
		//}
		auto _Ans = std::vector<char>(len);
		std::for_each(_Ans.begin(), _Ans.end(), [](auto& ch) {
			ch = templ[std::random_device{}() % templ.length()]; });
		return std::string(_Ans.begin(), _Ans.end());
	}



} // namespace util
