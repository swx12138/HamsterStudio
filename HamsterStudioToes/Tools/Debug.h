#pragma once

#include <stdexcept>
#include <string>
#include <vector>

namespace HamsterStudioToes::Debug
{
	class AssertException : public std::runtime_error
	{
	public:
		explicit AssertException(const std::string& message);
	};

	void Assert(bool condition, std::string const& message = "");

	void ErrorExit(std::vector<int> safeCodes);
}