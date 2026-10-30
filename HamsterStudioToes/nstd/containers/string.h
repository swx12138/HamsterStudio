#pragma once

#include <cstddef>
#include <ostream>

namespace toes::nstd {

	class String {
		char* data_;
		size_t cap_;
		size_t len_;
	public:
		explicit String(char const* str);
		String(String const& other);			// 原来没有拷贝构造，浅拷贝后必定 double free
		String(String&& other) noexcept;
		String& operator=(String const& other);
		String& operator=(String&& other) noexcept;
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
