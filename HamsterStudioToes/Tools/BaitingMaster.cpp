#include "./BaitingMaster.h"

#include "./Diagnostics.h"
#include "../Shapes/Rectangle.h"

using namespace HamsterStudioToes;

std::vector<ImageNamespace::Image> BaitingMaster::Baiting(ImageNamespace::Image const& img, int const cols)
{
	if (img.Valid()) {
		Diagnostics::Trace::TraceError(L"图像是空的");
		return {};
	}

	const auto height = img.Height();
	const auto cell_width = static_cast<decltype(height)>(height / 4.0 * 3.0);
	const auto total_cell_width = cell_width * cols;
	const auto width = img.Width();

	if (width < total_cell_width)
	{
		Diagnostics::Trace::TraceError(L"图像宽度不够");
		return {};
	}

#pragma omp parllel for num_threads(2)
	const auto left_start = (width - total_cell_width) / 2;
	std::vector<Image::Image> results(cols);
	for (int col = 0; col < cols; ++col)
	{
		Shapes::Rectangle rc{ left_start + cell_width * col, 0, cell_width, height };
		results[col] = img(rc);
	}
	Diagnostics::Trace::TraceInfo(std::format(L"Split {} images", results.size()));
	return results;
}