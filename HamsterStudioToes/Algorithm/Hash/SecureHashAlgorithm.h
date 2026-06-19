#pragma once

#include "./defines.h"

#include <string>

namespace HamsterStudioToes::Hash
{
    class SHA256 {
        uint8_t  m_data[64] = { 0 };
        size_t   m_dataLen = 0;
        uint64_t m_bitLen = 0;
        uint32_t m_state[8];
        bool     m_finalized = false;
    public:
        SHA256() { reset(); }

        void reset();

        void update(const uint8_t* data, size_t length);

        void update(const std::string& str);

        std::string final();

    private:
        void transform(const uint8_t block[64]);
    };

}