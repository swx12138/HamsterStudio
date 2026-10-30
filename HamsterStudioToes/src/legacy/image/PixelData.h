#pragma once

#define PixelDataNamespace toes::colors

namespace PixelDataNamespace {

	class PixelData {
		using DataType = double;
	public:
		double Red, Green, Blue;
	public:
		explicit PixelData() noexcept;
		explicit PixelData(DataType r = 0, DataType g = 0, DataType b = 0) noexcept;
		virtual ~PixelData() noexcept;

		double Brightness() const;

	};

}