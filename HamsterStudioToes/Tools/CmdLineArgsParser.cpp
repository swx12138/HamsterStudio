#include "CmdLineArgsParser.h"

#include <format>
#include <iostream>

using namespace HamsterStudio::Framework;

std::mutex CmdLineArgsParser::alloc_inst;

std::string_view CmdLineArgsParser::operator[](std::string_view key) const
{
	if (has_key(key))
	{
		return get_value(key);
	}
	throw std::exception(std::format("can't find key:{}", key).data());
}

inline bool CmdLineArgsParser::is_ready() const
{
	return !args.empty();
}

int CmdLineArgsParser::has_key(std::string_view key) const
{
	for (auto iter = args.begin(); iter != args.end(); iter++)
	{
		if (_Match_Key(key, *iter))
		{
			return static_cast<int>(iter - args.begin());
		}
	}
	return false;
}

std::string_view CmdLineArgsParser::get_value(std::string_view key) const
{
	for (auto iter = args.begin(); iter != args.end(); iter++)
	{
		if (_Match_Key(key, *iter))
		{
			if (++iter != args.end())
			{
				return *iter;
			}
			break;
		}
	}
	throw std::exception(std::format("no matched param with \"{}\".", key).data());
}

void CmdLineArgsParser::print_all() const
{
	for (auto const& arg : args)
	{
		std::cout << arg << std::endl;
	}
}

CmdLineArgsParser& CmdLineArgsParser::getInst()
{
	static CmdLineArgsParser m_Inst;
	if (!m_Inst.is_ready())
	{
		std::lock_guard<std::mutex> g(alloc_inst);
		if (!m_Inst.is_ready())
		{
			m_Inst = CmdLineArgsParser{ __argc, __argv };
		}
	}
	return m_Inst;
}

HamsterStudio::Framework::CmdLineArgsParser::CmdLineArgsParser(int argc, char** argv)
	: args(argv, argv + argc)
{
}

bool HamsterStudio::Framework::CmdLineArgsParser::_Match_Key(std::string_view key, std::string_view val) const
{
	return ('-' == val[0] && key == val.substr(1)) ||
		('-' == val[1] && key == val.substr(2));
}
