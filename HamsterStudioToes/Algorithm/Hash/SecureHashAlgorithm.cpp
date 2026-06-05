#include "SecureHashAlgorithm.h"

constexpr uint32_t SHA256_InitHash[] = {
	0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
	0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
};

constexpr uint32_t SHA256_Constants[] = {
	0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
	0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
	0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
	0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
	0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
	0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
	0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
	0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
	0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
	0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
	0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
	0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
	0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
	0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
	0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
	0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
};

#include <sstream>
#include <iomanip>

void HamsterStudioToes::Hash::SHA256::reset()
{
	m_dataLen = 0;
	m_bitLen = 0;
	for (auto i = 0u;i < 8;++i) {
		m_state[i] = SHA256_InitHash[i];
	}
	m_finalized = false;
}

void HamsterStudioToes::Hash::SHA256::update(const uint8_t* data, size_t length)
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

void HamsterStudioToes::Hash::SHA256::update(const std::string& str)
{
	update(reinterpret_cast<const uint8_t*>(str.data()), str.size());
}

std::string HamsterStudioToes::Hash::SHA256::final()
{
	if (!m_finalized) {
		// 总比特长度
		uint64_t totalBits = m_bitLen + static_cast<uint64_t>(m_dataLen) * 8;

		// 填充 1 + 零
		m_data[m_dataLen++] = 0x80;
		if (m_dataLen > 56) {
			while (m_dataLen < 64) m_data[m_dataLen++] = 0;
			transform(m_data);
			m_dataLen = 0;
		}
		while (m_dataLen < 56) m_data[m_dataLen++] = 0;

		// 附加 64 位大端比特长度
		for (int i = 0; i < 8; ++i) {
			m_data[56 + i] = static_cast<uint8_t>(totalBits >> (56 - 8 * i));
		}
		transform(m_data);

		m_finalized = true;
	}

	// 将 8 个状态字转为大端字节，格式化为十六进制字符串
	std::ostringstream oss;
	for (int i = 0; i < 8; ++i) {
		uint32_t s = m_state[i];
		for (int j = 0; j < 4; ++j) {
			uint8_t byte = static_cast<uint8_t>(s >> (24 - 8 * j));
			oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(byte);
		}
	}
	return oss.str();
}

static inline uint32_t rotr(uint32_t x, uint32_t n)
{
	return (x >> n) | (x << (32 - n));
}

void HamsterStudioToes::Hash::SHA256::transform(const uint8_t block[64])
{
	uint32_t w[64];
	// 准备消息调度 0..15
	for (int i = 0; i < 16; ++i) {
		w[i] = (static_cast<uint32_t>(block[i * 4]) << 24) |
			(static_cast<uint32_t>(block[i * 4 + 1]) << 16) |
			(static_cast<uint32_t>(block[i * 4 + 2]) << 8) |
			(static_cast<uint32_t>(block[i * 4 + 3]));
	}
	for (int i = 16; i < 64; ++i) {
		uint32_t s0 = rotr(w[i - 15], 7) ^ rotr(w[i - 15], 18) ^ (w[i - 15] >> 3);
		uint32_t s1 = rotr(w[i - 2], 17) ^ rotr(w[i - 2], 19) ^ (w[i - 2] >> 10);
		w[i] = w[i - 16] + s0 + w[i - 7] + s1;
	}

	uint32_t a = m_state[0];
	uint32_t b = m_state[1];
	uint32_t c = m_state[2];
	uint32_t d = m_state[3];
	uint32_t e = m_state[4];
	uint32_t f = m_state[5];
	uint32_t g = m_state[6];
	uint32_t h = m_state[7];

	for (int i = 0; i < 64; ++i) {
		uint32_t S1 = rotr(e, 6) ^ rotr(e, 11) ^ rotr(e, 25);
		uint32_t ch = (e & f) ^ (~e & g);
		uint32_t temp1 = h + S1 + ch + SHA256_Constants[i] + w[i];
		uint32_t S0 = rotr(a, 2) ^ rotr(a, 13) ^ rotr(a, 22);
		uint32_t maj = (a & b) ^ (a & c) ^ (b & c);
		uint32_t temp2 = S0 + maj;

		h = g;
		g = f;
		f = e;
		e = d + temp1;
		d = c;
		c = b;
		b = a;
		a = temp1 + temp2;
	}

	m_state[0] += a;
	m_state[1] += b;
	m_state[2] += c;
	m_state[3] += d;
	m_state[4] += e;
	m_state[5] += f;
	m_state[6] += g;
	m_state[7] += h;
}
