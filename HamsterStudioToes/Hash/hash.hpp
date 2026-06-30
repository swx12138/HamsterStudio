#pragma once

#include "./Base64.h"
#include "./MessageDigest.h"

namespace HamsterStudioToes::Hash
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
        {
            MD5 md5;
            md5.update(data.data(), data.size());
            return md5.final();
        }
            break;
        default:
            break;
        }
        return "null";
    }

}
