#pragma once

#include "./PixelData.h"

#include <memory>

#define PixelDataProcessorNamespace HamsterStudio::PixelData::Processors

namespace PixelDataProcessorNamespace {

	class PixelDataProcessor {
	public:
		using Pointer = std::shared_ptr<PixelDataProcessor>;

		virtual ~PixelDataProcessor() noexcept;

		virtual PixelData& Process(PixelData&) const;
		void SetNextProcessor(Pointer const& next);

	private:
		Pointer nextProcessor_;
	};

	// 全局调色
	class GlobalPixelDataProcessor : public PixelDataProcessor {
		double r_, g_, b_;
	public:
		explicit GlobalPixelDataProcessor(double r, double g, double b) noexcept;
		virtual ~GlobalPixelDataProcessor() noexcept;
		virtual PixelData& Process(PixelData& pixel) const;
	};

	// 区分曝光范围调色
	class ExposureRangeProcessor : public PixelDataProcessor {
		double minHighlight_, maxDark_;
	public:
		explicit ExposureRangeProcessor(double minHighlight, double maxDark) noexcept;
		virtual ~ExposureRangeProcessor() noexcept;

		virtual PixelData& Process(PixelData& pixel) const;

	};

}