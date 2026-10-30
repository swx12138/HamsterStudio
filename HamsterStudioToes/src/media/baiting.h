#pragma once

#include <vector>

#include <hamster/toes/image.h>

namespace toes
{
	class BaitingMaster
	{
	public:
		static std::vector<toes::image::Image> Baiting(toes::image::Image const& img, int const cols = 2);
	};
}