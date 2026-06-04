#include "./StreamReader.h"

HamsterStudioToes::Tools::StreamReader::StreamReader(std::istream& is) noexcept
	: stream_(is)
{
#ifdef _DEBUG
	const auto now_pos = stream_.tellg();
#endif // _DEBUG
}

size_t HamsterStudioToes::Tools::StreamReader::GetSize() const
{
	auto p = stream_.tellg();
	stream_.seekg(0, std::ios::end);
	auto endp = stream_.tellg();
	stream_.seekg(p);
	return static_cast<size_t>(endp);
}

size_t HamsterStudioToes::Tools::StreamReader::GetPostion() const
{
	auto p = stream_.tellg();
	return static_cast<size_t>(p);
}

bool HamsterStudioToes::Tools::StreamReader::Read(void* dest, size_t n) const
{
	return stream_.read((char*)dest, n).gcount() == n;
}

void HamsterStudioToes::Tools::StreamReader::SeekForward(int n) const
{
	stream_.seekg(n, std::ios::cur);
}

void HamsterStudioToes::Tools::StreamReader::SeekBackward(int n) const
{
#ifdef _DEBUG
	const auto old_pos = stream_.tellg();
#endif // _DEBUG

	stream_.seekg(-n, std::ios::cur);

#ifdef _DEBUG
	const auto now_pos = stream_.tellg();
#endif // _DEBUG

	return;
}
