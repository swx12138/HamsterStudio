#include "core/strings.h"

#include <Windows.h>
#include <algorithm>
#include <cctype>
#include <sstream>
#include <stdexcept>

std::string toes::strings::Huawei::utf8_to_string(std::string const& str)
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

std::string toes::strings::Huawei::string_to_utf8(std::string const& str)
{
	long long nwLen = ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, NULL, 0);

	wchar_t* pwBuf = new wchar_t[nwLen + 1u]; // Be sure to add 1, otherwise there will be a tail
	::ZeroMemory(pwBuf, nwLen * 2 + 2);

	::MultiByteToWideChar(CP_ACP, 0, str.c_str(), static_cast<int>(str.length()), pwBuf, static_cast<int>(nwLen));

	long long nLen = ::WideCharToMultiByte(CP_UTF8, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

	char* pBuf = new char[nLen + 1u];
	::ZeroMemory(pBuf, nLen + 1);

	::WideCharToMultiByte(CP_UTF8, 0, pwBuf, static_cast<int>(nwLen), pBuf, static_cast<int>(nLen), NULL, NULL);

	std::string retStr(pBuf);

	delete[] pwBuf;
	delete[] pBuf;

	pwBuf = NULL;
	pBuf = NULL;

	return retStr;
}

std::string toes::strings::to_string(std::wstring const& wstr)
{
	std::string result;
	int len = ::WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), static_cast<int>(wstr.size()), NULL, 0, NULL, NULL);

	char* buffer = new char[len + 1];
	::WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), static_cast<int>(wstr.size()), buffer, len, NULL, NULL);

	buffer[len] = '\0';
	result.append(buffer);

	delete[] buffer;
	return result;
}

std::wstring toes::strings::to_wstring(std::string const& str)
{
	std::wstring result;
	int len = ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), static_cast<int>(str.size()), NULL, 0);

	TCHAR* buffer = new TCHAR[len + 1];
	::MultiByteToWideChar(CP_ACP, 0, str.c_str(), static_cast<int>(str.size()), buffer, len);

	buffer[len] = '\0';
	result.append(buffer);

	delete[] buffer;
	return result;
}

std::string toes::strings::to_lower(std::string const &str)
{
	std::string result(str);
	// std::tolower 的实参必须是 unsigned char 提升后的值，否则高位字节是未定义行为
	std::transform(result.begin(), result.end(), result.begin(),
		[](unsigned char ch) { return static_cast<char>(std::tolower(ch)); });
	return result;
}

constexpr size_t Size_1K = 1024;
constexpr size_t Size_1M = 1024 * Size_1K;
constexpr size_t Size_1G = 1024 * Size_1M;
constexpr size_t Size_1T = 1024 * Size_1G;

#include <format>

std::string toes::strings::UnitedSize(size_t value)
{
	if (value >= Size_1T) {
		return std::format("{:.3f}TB", 1.0 * value / Size_1T);
	}
	else if (value >= Size_1G) {
		return std::format("{:.3f}GB", 1.0 * value / Size_1G);
	}
	else if (value >= Size_1M) {
		return std::format("{:.3f}MB", 1.0 * value / Size_1M);
	}
	return std::format("{:.3f}KB", 1.0 * value / Size_1K);
}

std::string toes::strings::Join(std::vector<std::string_view> const& vec, std::string delim)
{
	// 原实现对空容器解引用 begin()，是未定义行为
	std::string result;
	for (size_t i = 0; i < vec.size(); ++i) {
		if (i != 0) {
			result += delim;
		}
		result += vec[i];
	}
	return result;
}

std::vector<std::string> toes::strings::Split(std::string const& str, std::string delim)
{
	std::vector<std::string> _Ans;
	if (delim.empty()) {
		_Ans.push_back(str);
		return _Ans;
	}

	// 原实现直接取 _Tmp[0]，字符串里没有分隔符时会越界读
	size_t _Start = 0;
	for (;;) {
		size_t const _Pos = str.find(delim, _Start);
		if (_Pos == std::string::npos) {
			_Ans.push_back(str.substr(_Start));
			return _Ans;
		}
		_Ans.push_back(str.substr(_Start, _Pos - _Start));
		_Start = _Pos + delim.size();
	}
}

std::vector<std::string> toes::strings::SplitV2(std::string const& str, char const delim)
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

std::string toes::strings::Replace(std::string src, std::string const& which, std::string const& toWhat)
{
	// 空 needle 会让 find 永远命中位置 0，原实现因此死循环
	if (which.empty()) {
		return src;
	}

	size_t pos = 0;
	while ((pos = src.find(which, pos)) != std::string::npos) {
		src.replace(pos, which.size(), toWhat);
		pos += toWhat.size();	// 跳过刚写入的内容，避免 toWhat 含 which 时反复命中
	}
	return src;
}

toes::strings::ByteArray toes::strings::operator""_ba(const char* szData, const size_t len)
{
	ByteArray _Ans;
	for (int i = 0u; i < len; i++)
	{
		_Ans.push_back(szData[i]);
	}
	return _Ans;
}

toes::strings::ByteArray toes::strings::GetBytes(std::string const& str)
{
	return ByteArray(str.cbegin(), str.cend());
}

std::string toes::strings::Hex(ByteArray const& byts, bool upper)
{
	// 原实现用 "%2X"，十六进制宽度 2 会以空格填充，0x0A 会被写成 " a"
	char const* const digits = upper ? "0123456789ABCDEF" : "0123456789abcdef";
	std::string _Ans;
	_Ans.reserve(byts.size() * 2);
	for (auto byt : byts)
	{
		_Ans.push_back(digits[(byt >> 4) & 0x0f]);
		_Ans.push_back(digits[byt & 0x0f]);
	}
	return _Ans;
}

toes::strings::ByteArray toes::strings::Dehex(std::string const& str)
{
	auto const nibble = [](char ch) -> int {
		if (ch >= '0' && ch <= '9') return ch - '0';
		if (ch >= 'a' && ch <= 'f') return ch - 'a' + 10;
		if (ch >= 'A' && ch <= 'F') return ch - 'A' + 10;
		return -1;
	};

	// 原实现对奇数字符串会多读一个字符，对非法字符也会算出一个错误的值
	if (str.size() % 2 != 0) {
		throw std::invalid_argument("Dehex: 输入长度必须为偶数");
	}

	ByteArray _Ans;
	_Ans.reserve(str.size() / 2);
	for (size_t i = 0; i < str.size(); i += 2) {
		int const hi = nibble(str[i]);
		int const lo = nibble(str[i + 1]);
		if (hi < 0 || lo < 0) {
			throw std::invalid_argument("Dehex: 输入含非十六进制字符");
		}
		_Ans.push_back(static_cast<uint8_t>((hi << 4) | lo));
	}
	return _Ans;
}

bool toes::strings::isValidNum(std::string const& str)
{
	if (str.length() == 0) {
		return false;
	}
	if (str.length() != 1 && str[0] == '0') {
		return false;
	}
	// isdigit 同样要求实参可表示为 unsigned char
	return std::all_of(str.cbegin(), str.cend(),
		[](unsigned char ch) { return std::isdigit(ch) != 0; });
}
