#pragma once

#include <string>

namespace HamsterStudioToes
{
	constexpr std::string_view NoramlTempl{ "0123456789AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz_" };
	constexpr std::string_view NoramlTempl2{ "0123456789AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz_!@#$%^&*+-" };

	class RandomPasswordGenerator
	{
		std::string_view templ;
	public:
		explicit RandomPasswordGenerator(std::string_view _Templ);
		explicit RandomPasswordGenerator();

		std::string operator()(size_t len = 16);
	};
}