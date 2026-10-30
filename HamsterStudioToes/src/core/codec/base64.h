#pragma once

#include <stdexcept>
#include <cctype>
#include <vector>

namespace toes::base64
{
    std::string Encode(std::vector<uint8_t> const& bytes);

    std::vector<uint8_t> Decode(std::string const& base64);

}