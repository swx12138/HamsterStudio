#include "Debug.h"

HamsterStudioToes::Debug::AssertException::AssertException(const std::string& message)
	: std::runtime_error(message.data())
{
}

void HamsterStudioToes::Debug::Assert(bool condition, std::string const& message)
{
	if (!condition) {
		throw AssertException(message.data());
	}
}

