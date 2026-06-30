#pragma once

#define PASTE_IMAGE_UTILS_NAMESPACE toes::image

namespace PASTE_IMAGE_UTILS_NAMESPACE {

	enum class StretchMode { None, Fill, Uniform, UniformToFill };

	class ImagePaster
	{
	public:
		static void Paste(void* pCanvasMat, void const* pMat, StretchMode stretchMode);

	};
}