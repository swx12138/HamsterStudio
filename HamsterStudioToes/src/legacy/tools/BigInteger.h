#pragma once

#include <string>

namespace HamsterStudioToes
{
	std::string AddNumber(std::string num1, std::string num2);

	class BigInteger {
		using Reference = BigInteger&;
		using Const_Reference = const Reference;

	public:
		explicit BigInteger(std::string const& val);
		explicit BigInteger();
		explicit BigInteger(int val);

		void operator=(int val);
		BigInteger operator+(Const_Reference bigNumber);
		bool operator==(std::string const& val);
		operator std::string() const;

	private:
		std::string _Num;
	};
}