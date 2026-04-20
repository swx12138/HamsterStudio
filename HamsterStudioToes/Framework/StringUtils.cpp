#include "StringUtils.h"

#include <Windows.h>
#include <sstream>
#include <algorithm>

std::string HamsterStudioToes::StringUtils::Huawei::utf8_to_string(std::string const& str)
{
	auto nwLen = ::MultiByteToWideChar(CP_UTF8, 0, str.c_str(), -1, NULL, 0);

	wchar_t* pwBuf = new wchar_t[nwLen + 1u]; // Be sure to add 1, otherwise there will be a tail
	::memset(pwBuf, 0, static_cast<size_t>(nwLen * 2u + 2));

	::MultiByteToWideChar(CP_UTF8, 0, str.c_str(), static_cast<int>(str.length()), pwBuf, nwLen);

	auto nLen = ::WideCharToMultiByte(CP_ACP, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

	char* pBuf = new char[nLen + 1u];
	::memset(pBuf, 0, nLen + 1u);

	::WideCharToMultiByte(CP_ACP, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

	std::string retStr = pBuf;

	delete[] pBuf;
	delete[] pwBuf;

	pBuf = NULL;
	pwBuf = NULL;

	return retStr;
}

std::string HamsterStudioToes::StringUtils::Huawei::string_to_utf8(std::string const& str)
{
	long long nwLen = ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, NULL, 0);

	wchar_t* pwBuf = new wchar_t[nwLen + 1u]; // Be sure to add 1, otherwise there will be a tail
	::ZeroMemory(pwBuf, nwLen * 2 + 2);

	::MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), pwBuf, nwLen);

	long long nLen = ::WideCharToMultiByte(CP_UTF8, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

	char* pBuf = new char[nLen + 1u];
	::ZeroMemory(pBuf, nLen + 1);

	::WideCharToMultiByte(CP_UTF8, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

	std::string retStr(pBuf);

	delete[] pwBuf;
	delete[] pBuf;

	pwBuf = NULL;
	pBuf = NULL;

	return retStr;
}

std::string HamsterStudioToes::StringUtils::to_string(std::wstring const& wstr)
{
	std::string result;
	int len = ::WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.size(), NULL, 0, NULL, NULL);

	char* buffer = new char[len + 1];
	::WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.size(), buffer, len, NULL, NULL);

	buffer[len] = '\0';
	result.append(buffer);

	delete[] buffer;
	return result;
}

std::wstring HamsterStudioToes::StringUtils::to_wstring(std::string const& str)
{
	std::wstring result;
	int len = ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.size(), NULL, 0);

	TCHAR* buffer = new TCHAR[len + 1];
	::MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.size(), buffer, len);

	buffer[len] = '\0';
	result.append(buffer);

	delete[] buffer;
	return result;
}

std::string HamsterStudioToes::StringUtils::Join(std::vector<std::string_view> const& vec, std::string delim)
{
	std::ostringstream _Stream;
	_Stream << *vec.cbegin();
	std::for_each(vec.cbegin() + 1, vec.cend(), [=, &_Stream](auto const val)
		{ _Stream << delim << val; });
	return _Stream.str();
}

std::vector<std::string> HamsterStudioToes::StringUtils::Split(std::string const& str, std::string delim)
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

std::vector<std::string> HamsterStudioToes::StringUtils::SplitV2(std::string const& str, char const delim)
{
	std::vector<std::string> ret;
	std::stringstream ss(str);
	std::string ea;
	while (std::getline(ss, ea, delim))
	{
		ret.push_back(ea);
	}
	return ret;
}

std::string HamsterStudioToes::StringUtils::Replace(std::string src, std::string const& which, std::string const& toWhat)
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

HamsterStudioToes::StringUtils::ByteArray HamsterStudioToes::StringUtils::operator""_ba(const char* szData, const size_t len)
{
	ByteArray _Ans;
	for (int i = 0u; i < len; i++)
	{
		_Ans.push_back(szData[i]);
	}
	return _Ans;
}

HamsterStudioToes::StringUtils::ByteArray HamsterStudioToes::StringUtils::GetBytes(std::string const& str)
{
	return ByteArray(str.cbegin(), str.cend());
}

std::string HamsterStudioToes::StringUtils::Hex(ByteArray const& byts, bool upper)
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

HamsterStudioToes::StringUtils::ByteArray HamsterStudioToes::StringUtils::Dehex(std::string const& str)
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

bool HamsterStudioToes::StringUtils::isValidNum(std::string const& str)
{
	if (str.length() == 0) {
		return false;
	}
	if (str.length() != 1 && str[0] == '0') {
		return false;
	}
	return all_of(str.cbegin(), str.cend(), isdigit);
}
