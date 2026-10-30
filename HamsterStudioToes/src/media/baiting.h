#pragma once

#include <vector>

#include <hamster/toes/image.h>

namespace HamsterStudioToes
{
	class BaitingMaster
	{
	public:
		static std::vector<ImageNamespace::Image> Baiting(ImageNamespace::Image const& img, int const cols = 2);
	};
}