#pragma once

#include <ostream>

namespace toes::nstd {

	class String {
		char* data_;
		size_t cap_;
		size_t len_;
	public:
		explicit String(char const* str);
		~String();

		char const* Data() const;
		size_t Length() const;
		size_t Size() const;

		bool Append(String const& str);
		void Reverse();
		void ToLower();
		void ToUpper();

	private:
		void Release();	
		void MoveFrom(String && str);
		bool CopyFrom(String const& str);

	};

	std::ostream& operator<<(std::ostream& os, String const& str);


}
