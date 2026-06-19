#include "string.h"

#include <memory>
#include <algorithm>

toes::nstd::String::String(char const* str)
	:data_(nullptr)
	, len_(0)
	, cap_(0)
{
	if (str == nullptr) return;

	len_ = 0;
	while (str[len_] != '\0') len_++;

	cap_ = len_ + 1;
	data_ = new char[cap_];
	for (size_t i = 0; i < len_; i++) {
		data_[i] = str[i];
	}
	data_[len_] = '\0';
}

toes::nstd::String::~String()
{
	Release();
}

char const* toes::nstd::String::Data() const
{
	return data_;
}

size_t toes::nstd::String::Length() const
{
	return len_;
}

size_t toes::nstd::String::Size() const
{
	return cap_;
}

bool toes::nstd::String::Append(String const& str)
{
	if (str.data_ == nullptr) return false;
	if (data_ == nullptr) {
		if (CopyFrom(str)) {
			return true;
		}
		else {
			// not reachable
		}
	}
	else {
		const auto new_len = len_ + str.len_;
		const auto new_data_ = new char[new_len + 1];
		::memcpy(new_data_, data_, len_);
		::memcpy(new_data_ + len_, str.data_, str.len_);
		new_data_[new_len] = '\0';

		Release();
		data_ = new_data_;
		len_ = new_len;
		cap_ = new_len + 1;
	}
	return true;
}

void toes::nstd::String::Reverse()
{
	std::reverse(data_, data_ + len_);
}

void toes::nstd::String::ToLower()
{
	for (int i = 0;i < len_;i++)
	{
		if (islower(data_[i]))
		{
			data_[i] = data_[i] - 'a' + 'A';
		}
	}
}

void toes::nstd::String::ToUpper()
{
	for (int i = 0;i < len_;i++)
	{
		if (isupper(data_[i]))
		{
			data_[i] = data_[i] - 'A' + 'a';
		}
	}
}

void toes::nstd::String::Release()
{
	if (data_ != nullptr) {
		delete[] data_;
	}
	cap_ = len_ = 0;
}

void toes::nstd::String::MoveFrom(String && str)
{
	Release();

	data_ = str.data_;
	len_ = str.len_;
	cap_ = str.cap_;

	str.data_ = nullptr;
	str.Release();
}

bool toes::nstd::String::CopyFrom(String const& str)
{
	Release();

	if (str.data_ == nullptr) return false;

	data_ = new char[str.cap_];
	::memcpy(data_, str.data_, str.cap_);
	len_ = str.len_;
	cap_ = str.cap_;

	return true;
}

std::ostream& toes::nstd::operator<<(std::ostream& os, String const& str)
{
	os << str.Data();
	return os;
}
