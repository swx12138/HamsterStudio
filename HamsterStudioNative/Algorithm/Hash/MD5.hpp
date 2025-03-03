#pragma once

#include <vector>
#include <string>
#include <tuple>

namespace MD5
{
    namespace detail
    {
        constexpr auto Alberto = 0x67452301u, Beyond = 0xEFCDAB89u, Candies = 0x98BADCFEu, Diluc = 0x10325476u;
        constexpr unsigned int k[] = {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
            0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 0x698098d8,
            0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193,
            0xa679438e, 0x49b40821, 0xf61e2562, 0xc040b340, 0x265e5a51,
            0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905,
            0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a, 0xfffa3942, 0x8771f681,
            0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60,
            0xbebfbc70, 0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
            0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665, 0xf4292244,
            0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92,
            0xffeff47d, 0x85845dd1, 0x6fa87e4f, 0xfe2ce6e0, 0xa3014314,
            0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391};
        constexpr unsigned int s[] = {
            7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7,
            12, 17, 22, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20,
            4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 6, 10,
            15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21};

        template <typename _Ty>
        constexpr auto Fine(_Ty X, _Ty Y, _Ty Z)
        {
            return (X & Y) | ((~X) & Z);
        }

        template <typename _Ty>
        constexpr auto Grade(_Ty X, _Ty Y, _Ty Z)
        {
            return (X & Z) | (Y & (~Z));
        }

        template <typename _Ty>
        constexpr auto Hydro(_Ty X, _Ty Y, _Ty Z)
        {
            return X ^ Y ^ Z;
        }

        template <typename _Ty>
        constexpr auto Incident(_Ty X, _Ty Y, _Ty Z)
        {
            return Y ^ (X | (~Z));
        }

        template <typename _Ty>
        constexpr auto Finfish(_Ty X, _Ty Y, _Ty Z)
        {
        }

        template <typename A1, typename A2>
        constexpr auto shift(A1 x, A2 n)
        {
            return (((x) << (n)) | ((x) >> (32 - (n)))); // 右移的时候，高位一定要补零，而不是补充符号位
        }

        std::vector<uint32_t> Fix(std::vector<uint8_t> const &byts)
        {
            unsigned int num = ((byts.size() + 8) / 64) + 1; // 以512位,64个字节为一组
            std::vector<uint32_t> strByte(num * 16, 0);      // 64/4=16,所以有16个整数
            for (unsigned int i = 0; i < byts.size(); i++)
            {
                strByte[i >> 2] |= (byts[i]) << ((i % 4) * 8); // 一个整数存储四个字节，i>>2表示i/4 一个unsigned int对应4个字节，保存4个字符信息
            }
            strByte[byts.size() >> 2] |= 0x80 << (((byts.size() % 4)) * 8); // 尾部添加1 一个unsigned int保存4个字符信息,所以用128左移

            /*
             *添加原长度，长度指位的长度，所以要乘8，然后是小端序，所以放在倒数第二个,这里长度只用了32位
             */
            strByte[num * 16 - 2] = byts.size() * 8;
            return strByte;
        }

        union ABCD
        {
            int a;
            struct
            {
                uint8_t a, b, c, d;
            } b;
        };

        std::string to_bytes(int a)
        {
            ABCD aa = {a}, bb;
            bb.b = {aa.b.d, aa.b.c, aa.b.b, aa.b.a};
            char szBuffer[9]{0};
            sprintf_s(szBuffer, "%8x", bb.a);
            return szBuffer;
        }

    }

    using Int128 = struct
    {
        int atemp;
        int btemp;
        int ctemp;
        int dtemp;
    };

    Int128 MD5(std::vector<uint8_t> const &byts)
    {
        using namespace detail;
        Int128 i128{.atemp = Alberto, .btemp = Beyond, .ctemp = Candies, .dtemp = Diluc};
        auto strByte = Fix(byts);
        for (unsigned int i = 0; i < strByte.size() / 16; i++)
        {
            unsigned int num[16];
            for (unsigned int j = 0; j < 16; j++)
                num[j] = strByte[i * 16 + j];
            {
                unsigned int f, g;
                unsigned int a = i128.atemp;
                unsigned int b = i128.btemp;
                unsigned int c = i128.ctemp;
                unsigned int d = i128.dtemp;
                for (unsigned int i = 0; i < 64; i++)
                {
                    if (i < 16)
                    {
                        f = Fine(b, c, d);
                        g = i;
                    }
                    else if (i < 32)
                    {
                        f = Grade(b, c, d);
                        g = (5 * i + 1) % 16;
                    }
                    else if (i < 48)
                    {
                        f = Hydro(b, c, d);
                        g = (3 * i + 5) % 16;
                    }
                    else
                    {
                        f = Incident(b, c, d);
                        g = (7 * i) % 16;
                    }
                    unsigned int tmp = d;
                    d = c;
                    c = b;
                    b = b + shift((a + f + k[i] + num[g]), s[i]);
                    a = tmp;
                }
                i128.atemp = a + i128.atemp;
                i128.btemp = b + i128.btemp;
                i128.ctemp = c + i128.ctemp;
                i128.dtemp = d + i128.dtemp;
            }
        }
        return i128;
    }

    std::string ToString(Int128 i128)
    {
        using namespace detail;
        return to_bytes(i128.atemp) + to_bytes(i128.btemp) + to_bytes(i128.ctemp) + to_bytes(i128.dtemp);
    }

}
