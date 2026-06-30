#pragma once

#include "./defines.h"

#include <string>

namespace HamsterStudioToes::Hash
{
    class MD5 {
        uint8_t  m_data[64] = { 0 };
        size_t   m_dataLen = 0;
        uint64_t m_bitLen = 0;
        uint32_t m_state[4];
        bool     m_finalized = false;
    public:
        MD5() { reset(); }

        void reset();

        void update(const uint8_t* data, size_t length);

        void update(const std::string& str);

        std::string final();

    private:
        void transform(const uint8_t block[64]);
    };

    int md5_test_main();
}
