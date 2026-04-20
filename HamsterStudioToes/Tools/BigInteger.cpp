#include "BigInteger.h"

std::string HamsterStudioToes::AddNumber(std::string num1, std::string num2)
{
	std::string _Ans;
	auto iter1 = num1.crbegin(), iter2 = num2.crbegin();
	int _Jin = 0;
	auto _Fix_Value = [&](char& value)
		{
			if (value > '9')
			{
				_Jin = (value - '0') / 10;
				value -= 10;
			}
			else
			{
				_Jin = 0;
			}
		};
	while (iter1 != num1.crend() && iter2 != num2.crend())
	{
		char value = *(iter1++) - '0' + *(iter2++) + _Jin;
		_Fix_Value(value);
		_Ans.push_back(value);
	}
	while (iter1 != num1.crend())
	{
		char value = *(iter1++) + _Jin;
		_Fix_Value(value);
		_Ans.push_back(value);
	}
	while (iter2 != num2.crend())
	{
		char value = *(iter2++) + _Jin;
		_Fix_Value(value);
		_Ans.push_back(value);
	}
	if (_Jin != 0)
	{
		_Ans.push_back(_Jin + '0');
	}
	reverse(_Ans.begin(), _Ans.end());
	return _Ans;
}

HamsterStudioToes::BigInteger::BigInteger(std::string const& val)
	: _Num(val) {
}

HamsterStudioToes::BigInteger::BigInteger() :
	BigInteger("0") {
}

HamsterStudioToes::BigInteger::BigInteger(int val)
{
	*this = val;
}

void HamsterStudioToes::BigInteger::operator=(int val)
{
	_Num = std::to_string(val);
}

HamsterStudioToes::BigInteger HamsterStudioToes::BigInteger::operator+(Const_Reference bigNumber)
{
	return BigInteger{ AddNumber(_Num, bigNumber._Num) };
}

bool HamsterStudioToes::BigInteger::operator==(std::string const& val)
{
	return val == _Num;
}

HamsterStudioToes::BigInteger::operator std::string() const
{
	return _Num;
}
