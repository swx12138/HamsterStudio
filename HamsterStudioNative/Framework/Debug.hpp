#pragma once

#include <string>

namespace Debug
{
	void Assert(bool condition, std::string const &message = "")
	{
		if (!condition) {
			throw std::exception(message.data());
		}
	}
}