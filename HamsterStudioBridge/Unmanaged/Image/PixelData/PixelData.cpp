#include "PixelData.h"

using namespace PixelDataNamespace;

PixelData::PixelData() noexcept
	: Red(1), Green(1), Blue(1)
{
}

PixelData::PixelData(DataType r, DataType g, DataType b) noexcept
	: Red(r), Green(g), Blue(b)
{
}

PixelData::~PixelData() noexcept
{
}

double PixelData::Brightness() const
{
	// 标准人眼感知亮度:Y=0.299⋅R+0.587⋅G+0.114⋅B
	return 0.299 * Red + 0.587 * Green + 0.114 * Blue;
}