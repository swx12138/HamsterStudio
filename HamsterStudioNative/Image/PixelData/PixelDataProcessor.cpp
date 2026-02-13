#include "PixelDataProcessor.h"

#include <stdexcept>

using namespace PixelDataNamespace;
using namespace PixelDataProcessorNamespace;

PixelDataProcessor::~PixelDataProcessor() noexcept
{
}

void PixelDataProcessor::SetNextProcessor(Pointer const& next) {
	if (next.get() == this) {
		throw std::runtime_error("not support");
	}
	nextProcessor_ = next;
}

PixelData& PixelDataProcessor::Process(PixelData& pixel) const
{
	if (nextProcessor_) {
		return nextProcessor_->Process(pixel);
	}
	else {
		return pixel;
	}
}

GlobalPixelDataProcessor::GlobalPixelDataProcessor(double r, double g, double b) noexcept
	: r_(r), g_(g), b_(b)
{
}

GlobalPixelDataProcessor::~GlobalPixelDataProcessor() noexcept
{
}

PixelData& GlobalPixelDataProcessor::Process(PixelData& pixel) const
{
	pixel.Red *= r_;
	pixel.Green *= g_;
	pixel.Blue *= b_;
	return PixelDataProcessor::Process(pixel);
}

ExposureRangeProcessor::ExposureRangeProcessor(double minHighlight, double maxDark) noexcept
	: minHighlight_(minHighlight), maxDark_(maxDark)
{
}

ExposureRangeProcessor::~ExposureRangeProcessor() noexcept 
{
}

PixelData& ExposureRangeProcessor::Process(PixelData& pixel) const
{

}
