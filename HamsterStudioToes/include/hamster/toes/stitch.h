#pragma once

#include <hamster/toes/image.h>
#include <hamster/toes/layout.h>
#include "stitch/paste.h"

#include <hamster/toes/export.h>

#include <vector>
#include <array>

namespace ImageNamespace
{
	using namespace PASTE_IMAGE_UTILS_NAMESPACE;

	/// 拼接模式：None=全部, Portrait=仅竖屏, Landscape=仅横屏
	enum class ImageStitcheMode { None, Portrait, Landscape };

	TOES_API Image stitch(std::vector<void*> mats, StretchMode mode, ImageShape size, ImageUtilsNamespace::ImageLayout layout, int spacing);

	int stitch_cli_main(std::vector<std::string> const& args);

}