
#pragma once

#include <stdexcept>
#include <cctype>
#include <vector>

#include "../../Framework/Debug.hh"

namespace Base64
{
    namespace detail
    {
        constexpr inline char to_base64(uint8_t u8)
        {
            if (u8 < 26)
                return 'A' + u8;
            else if (u8 < 52)
                return 'a' + u8 - 26;
            else if (u8 < 62)
                return '0' + u8 - 52;
            else if (u8 == 62)
                return '+';
            else if (u8 == 63)
                return '/';
            throw std::out_of_range("u8 shall less than 64.");
        }

        inline uint8_t from_base64(char ch)
        {
            if (isupper(ch))
                return ch - 'A';
            else if (islower(ch))
                return ch - 'a' + 26;
            else if (isdigit(ch))
                return ch - '0' + 52;
            else if ('+' == ch)
                return 62;
            else if ('/' == ch)
                return 63;
            else if ('=' == ch)
                return 0;
            throw std::out_of_range("ch shall be alpha or digit.");
        }

    }

    std::string Encode(std::vector<uint8_t> const &bytes)
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

    std::vector<uint8_t> Decode(std::string const &base64)
    {
        using namespace detail;
        std::vector<uint8_t> _Ans;
        const auto _Len = base64.size();
        Debug::Assert(_Len % 4 == 0);
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
        return _Ans;
    }

}