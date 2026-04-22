#pragma once

#include <string>
#include <vector>

namespace HamsterStudioToes::StringUtils
{
	namespace Huawei
	{
		//@brief:UTF8 to string
		std::string utf8_to_string(std::string const& str);

		//@brief:string to UTF8
		std::string string_to_utf8(std::string const& str);
	}

	std::string to_string(std::wstring const& wstr);
	std::wstring to_wstring(std::string const& str);

	std::string Join(std::vector<std::string_view> const& vec, std::string delim);
	std::vector<std::string> Split(std::string const& str, std::string delim);
	std::vector<std::string> SplitV2(std::string const& str, char const delim);
	std::string Replace(std::string src, std::string const& which, std::string const& toWhat);

	using ByteArray = std::vector<uint8_t>; // std::basic_string<unsigned char>;
	ByteArray operator""_ba(const char* szData, const size_t len);
	ByteArray GetBytes(std::string const& str);

	std::string Hex(ByteArray const& byts, bool upper = false);
	ByteArray Dehex(std::string const& str);

	bool isValidNum(std::string const& str);
}