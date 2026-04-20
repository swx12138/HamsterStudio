#pragma once

#include <stdexcept>
#include <cctype>
#include <vector>

#include "../../Framework/Debug.h"

namespace HamsterStudioToes::Base64
{
    std::string Encode(std::vector<uint8_t> const& bytes);

    std::vector<uint8_t> Decode(std::string const& base64);

}