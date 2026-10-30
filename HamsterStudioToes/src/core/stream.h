#pragma once

#include <istream>

namespace HamsterStudioToes::Tools {

	class  StreamReader {
		std::istream& stream_;
	public:
		explicit StreamReader(std::istream& is) noexcept;

		size_t GetSize() const;
		size_t GetPostion() const;

		template<typename Ty>
		std::pair<Ty, bool> Read() const
		{
			constexpr auto TySize = sizeof(Ty);
			std::pair<Ty, bool> result;
			if (stream_.eof())
			{
				result.second = false;
			}
			else {
				result.second = stream_.read((char*)(&result.first), TySize).gcount() == TySize;
			}
			return result;
		}

		bool Read(void* dest, size_t n) const;

		void SeekForward(int n) const;
		void SeekBackward(int n) const;

	};

}
