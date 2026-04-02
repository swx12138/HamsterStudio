#pragma once

#include "Base64.hpp"
#include "MD5.hpp"

namespace Hash
{
    enum class HashAlgorithm
    {
        Base64,
        MD5
    };

    std::string Hash(HashAlgorithm _HashAlgorithm, std::vector<uint8_t> const &data)
    {
        switch (_HashAlgorithm)
        {
        case HashAlgorithm::Base64:
            return Base64::Encode(data);
            break;
        case HashAlgorithm::MD5:
            return MD5::ToString(MD5 ::MD5(data));
            break;
        default:
            break;
        }
        return "null";
    }

}
