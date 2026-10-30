#include "core/codec/base64.h"

#include <cctype>
#include <stdexcept>

namespace detail
{
    constexpr inline char to_base64(uint8_t u8)
    {
        if (u8 < 26)
            return static_cast<char>('A' + u8);
        else if (u8 < 52)
            return static_cast<char>('a' + u8 - 26);
        else if (u8 < 62)
            return static_cast<char>('0' + u8 - 52);
        else if (u8 == 62)
            return '+';
        else if (u8 == 63)
            return '/';
        throw std::out_of_range("u8 shall less than 64.");
    }

    inline uint8_t from_base64(char ch)
    {
        // ctype 系列要求实参可表示为 unsigned char，否则高位字节是未定义行为
        auto const u = static_cast<unsigned char>(ch);
        if (std::isupper(u))
            return static_cast<uint8_t>(u - 'A');
        else if (std::islower(u))
            return static_cast<uint8_t>(u - 'a' + 26);
        else if (std::isdigit(u))
            return static_cast<uint8_t>(u - '0' + 52);
        else if ('+' == ch)
            return 62;
        else if ('/' == ch)
            return 63;
        else if ('=' == ch)
            return 0;
        throw std::out_of_range("ch shall be alpha or digit.");
    }

}

std::string toes::base64::Encode(std::vector<uint8_t> const& bytes)
{
    using namespace detail;
    std::vector<char> _Ans;

    const auto _Len = bytes.size();
    auto i = 0u;
    for (; i + 2 < _Len; i += 3)
    {
        auto a = bytes[i], b = bytes[i + 1u], c = bytes[i + 2u];
        _Ans.push_back(to_base64(a >> 2));
        _Ans.push_back(to_base64(
            (((a << 4) | (b >> 4))) & 0x3f));
        _Ans.push_back(to_base64(
            (((b << 2) | (c >> 6))) & 0x3f));
        _Ans.push_back(to_base64(c & 0x3f));
    }

    switch (_Len - i)
    {
    case 0:
        break;
    case 1:
        _Ans.push_back(to_base64(bytes[i] >> 2));
        _Ans.push_back(to_base64(
            (bytes[i] << 4) & 0x3f));
        _Ans.push_back('=');
        _Ans.push_back('=');
        break;
    case 2:
        _Ans.push_back(to_base64(bytes[i] >> 2));
        _Ans.push_back(to_base64(
            (bytes[i] << 4 | bytes[i + 1u] >> 4) & 0x3f));
        _Ans.push_back(to_base64(
            (bytes[i + 1u] << 2) & 0x3f));
        _Ans.push_back('=');
        break;
    default:
        throw std::out_of_range("what's wrong with you");
    }
    return std::string(_Ans.begin(), _Ans.end());
}

#include "core/diagnostics.h"

std::vector<uint8_t> toes::base64::Decode(std::string const& base64)
{
    using namespace detail;
    std::vector<uint8_t> _Ans;
    const auto _Len = base64.size();

    // 长度不是 4 的倍数时无法确定填充位置，宁可拒绝也不要静默丢字节
    if (_Len % 4 != 0) {
        throw std::invalid_argument("Base64::Decode: 输入长度必须是 4 的倍数");
    }

    // 结尾的 '=' 表示被省略的字节数，解完要裁掉。原实现把 '=' 当 0 参与解码，
    // 于是 "aGVsbG8=" 会多出一个 0x00 字节。
    size_t _Pad = 0;
    if (_Len != 0 && base64[_Len - 1] == '=') {
        _Pad = (_Len >= 2 && base64[_Len - 2] == '=') ? 2u : 1u;
    }
    for (auto i = 0u; i + 3 < _Len; i += 4)
    {
        auto a = from_base64(base64[i]),
            b = from_base64(base64[i + 1u]),
            c = from_base64(base64[i + 2u]),
            d = from_base64(base64[i + 3u]);
        _Ans.emplace_back(a << 2 | b >> 4);
        _Ans.emplace_back(b << 4 | c >> 2);
        _Ans.emplace_back(c << 6 | d);
    }
    if (_Pad > _Ans.size()) {
        throw std::invalid_argument("Base64::Decode: 填充长度超过可用数据");
    }
    _Ans.resize(_Ans.size() - _Pad);
    return _Ans;
}
