#include "Keygen.h"

#include <random>

toes::RandomPasswordGenerator::RandomPasswordGenerator(std::string_view _Templ)
	: templ(_Templ) {
}

toes::RandomPasswordGenerator::RandomPasswordGenerator()
	: RandomPasswordGenerator(NoramlTempl) {
}

std::string toes::RandomPasswordGenerator::operator()(size_t len)
{
	std::string str;
	str.resize(len);
	std::mt19937 gen(std::random_device{}());
	std::uniform_int_distribution<> dist(0, static_cast<int>(templ.length() - 1));
	for (auto& ch : str)
	{
		ch = templ[dist(gen)];
	}

	return str;
}