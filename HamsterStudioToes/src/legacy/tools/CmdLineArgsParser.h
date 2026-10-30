#pragma once

#include <vector>
#include <string_view>
#include <mutex>

namespace HamsterStudio::Framework
{

	class CmdLineArgsParser
	{
	public:
		using Args = std::vector<std::string_view>;

		std::string_view operator[](std::string_view key) const;

		inline bool is_ready() const;
		int has_key(std::string_view key) const;

		std::string_view get_value(std::string_view key) const;

		void print_all() const;

		static CmdLineArgsParser& getInst();

	private:
		CmdLineArgsParser() {}
		CmdLineArgsParser(int argc, char** argv);

		bool _Match_Key(std::string_view key, std::string_view val) const;

	private:
		static std::mutex alloc_inst;
		Args args;
	};

}