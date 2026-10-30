#include "core/codec/md5.h"

constexpr uint32_t MD5_InitState[] = {
	0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476
};

constexpr uint32_t MD5_T[] = {
	0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
	0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
	0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
	0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
	0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
	0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
	0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
	0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
	0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
	0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
	0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
	0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
	0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
	0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
	0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
	0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
};

constexpr unsigned int MD5_Shift[] = {
	7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
	5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
	4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
	6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21
};

#include <sstream>
#include <iomanip>

void toes::hash::MD5::reset()
{
	m_dataLen = 0;
	m_bitLen = 0;
	for (auto i = 0u; i < 4; ++i) {
		m_state[i] = MD5_InitState[i];
	}
	m_finalized = false;
}

void toes::hash::MD5::update(const uint8_t* data, size_t length)
{
	if (m_finalized) return;
	for (size_t i = 0; i < length; ++i) {
		m_data[m_dataLen++] = data[i];
		if (m_dataLen == 64) {
			transform(m_data);
			m_bitLen += 512;
			m_dataLen = 0;
		}
	}
}

void toes::hash::MD5::update(const std::string& str)
{
	update(reinterpret_cast<const uint8_t*>(str.data()), str.size());
}

static inline uint32_t rotl(uint32_t x, uint32_t n)
{
	return (x << n) | (x >> (32 - n));
}

std::string toes::hash::MD5::final()
{
	if (!m_finalized) {
		uint64_t totalBits = m_bitLen + static_cast<uint64_t>(m_dataLen) * 8;

		// 填充 1 + 零
		m_data[m_dataLen++] = 0x80;
		if (m_dataLen > 56) {
			while (m_dataLen < 64) m_data[m_dataLen++] = 0;
			transform(m_data);
			m_dataLen = 0;
		}
		while (m_dataLen < 56) m_data[m_dataLen++] = 0;

		// MD5 使用小端序附加 64 位比特长度
		for (int i = 0; i < 8; ++i) {
			m_data[56 + i] = static_cast<uint8_t>(totalBits >> (8 * i));
		}
		transform(m_data);

		m_finalized = true;
	}

	// MD5 输出为小端序十六进制字符串
	std::ostringstream oss;
	for (int i = 0; i < 4; ++i) {
		uint32_t s = m_state[i];
		for (int j = 0; j < 4; ++j) {
			uint8_t byte = static_cast<uint8_t>(s >> (8 * j));
			oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(byte);
		}
	}
	return oss.str();
}

static inline uint32_t F(uint32_t x, uint32_t y, uint32_t z)
{
	return (x & y) | (~x & z);
}

static inline uint32_t G(uint32_t x, uint32_t y, uint32_t z)
{
	return (x & z) | (y & ~z);
}

static inline uint32_t H(uint32_t x, uint32_t y, uint32_t z)
{
	return x ^ y ^ z;
}

static inline uint32_t I(uint32_t x, uint32_t y, uint32_t z)
{
	return y ^ (x | ~z);
}

void toes::hash::MD5::transform(const uint8_t block[64])
{
	// MD5 消息字为小端序
	uint32_t M[16];
	for (int i = 0; i < 16; ++i) {
		M[i] = (static_cast<uint32_t>(block[i * 4])) |
			(static_cast<uint32_t>(block[i * 4 + 1]) << 8) |
			(static_cast<uint32_t>(block[i * 4 + 2]) << 16) |
			(static_cast<uint32_t>(block[i * 4 + 3]) << 24);
	}

	uint32_t a = m_state[0];
	uint32_t b = m_state[1];
	uint32_t c = m_state[2];
	uint32_t d = m_state[3];

	for (int i = 0; i < 64; ++i) {
		uint32_t f;
		unsigned int g;
		if (i < 16) {
			f = F(b, c, d);
			g = i;
		}
		else if (i < 32) {
			f = G(b, c, d);
			g = (5 * i + 1) % 16;
		}
		else if (i < 48) {
			f = H(b, c, d);
			g = (3 * i + 5) % 16;
		}
		else {
			f = I(b, c, d);
			g = (7 * i) % 16;
		}

		uint32_t temp = d;
		d = c;
		c = b;
		b = b + rotl(a + f + MD5_T[i] + M[g], MD5_Shift[i]);
		a = temp;
	}

	m_state[0] += a;
	m_state[1] += b;
	m_state[2] += c;
	m_state[3] += d;
}

#include <iostream>

int toes::hash::md5_test_main()
{
	auto test_str = "hello,world";
	toes::hash::MD5 md5;
	md5.update(test_str);
	std::cout << md5.final() << std::endl;
	return 0;
}
