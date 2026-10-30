#include "media/baiting.h"

#include "core/diagnostics.h"
#include <hamster/toes/geometry.h>

using namespace HamsterStudioToes;

std::vector<ImageNamespace::Image> BaitingMaster::Baiting(ImageNamespace::Image const& img, int const cols)
{
	if (!img.Valid()) {
		Diagnostics::Trace::TraceError(L"图像是空的");
		return {};
	}

	const auto width = img.Width();
	auto cell_height = img.Height();
	auto cell_width = static_cast<decltype(cell_height)>(cell_height / 4.0 * 3.0);
	auto total_cell_width = cell_width * cols;

	// 宽度不足时，降低 cell_height 来适配，默认使用图像中间区域
	if (width < total_cell_width)
	{
		cell_height = static_cast<decltype(cell_height)>(width * 4.0 / (3.0 * cols));
		cell_width = static_cast<decltype(cell_height)>(cell_height / 4.0 * 3.0);
		total_cell_width = cell_width * cols;
		Diagnostics::Trace::TraceInfo(std::format(L"图像宽度不足，已调整 cell_height 至 {}", cell_height));
	}

	const auto left_start = (width - total_cell_width) / 2;
	const auto top = (img.Height() - cell_height) / 2;
	std::vector<Image::Image> results(cols);
#pragma omp parallel for num_threads(2)
	for (int col = 0; col < cols; ++col)
	{
		Shapes::Rectangle rc{ left_start + cell_width * col, top, cell_width, cell_height };
		results[col] = img(rc);
	}
	Diagnostics::Trace::TraceInfo(std::format(L"Split {} images", results.size()));
	return results;
}