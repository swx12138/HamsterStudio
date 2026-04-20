#pragma once

#include <stdexcept>
#include <string>

namespace HamsterStudioToes::Debug
{
	class AssertException : public std::runtime_error
	{
	public:
		explicit AssertException(const std::string& message);
	};

	void Assert(bool condition, std::string const& message = "");
}