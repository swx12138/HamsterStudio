#pragma once

namespace toes::image {

	enum class StretchMode { None, Fill, Uniform, UniformToFill };

	class ImagePaster
	{
	public:
		static void Paste(void* pCanvasMat, void const* pMat, StretchMode stretchMode);

	};
}