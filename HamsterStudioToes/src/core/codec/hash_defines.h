#pragma once

#include <cstdint>

namespace toes::hash
{
#pragma pack(push, 1)

	union Hash256
	{
		uint8_t Byte[32];
		uint16_t Word[16];
		uint32_t Dword[8];
		uint64_t Qword[4];
	};

#pragma pack(pop)

}