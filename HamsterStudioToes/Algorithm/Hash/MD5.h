#pragma once

#include <vector>
#include <string>

namespace HamsterStudioToes::MD5
{
    using Int128 = struct { unsigned int atemp, btemp, ctemp, dtemp; };

    std::string ToString(Int128 i128);

    Int128 MD5(std::vector<uint8_t> const& byts);

}
