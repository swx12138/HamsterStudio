#pragma once

#include "../Win32Utils.h"

#include <memory>

namespace Win32Namespace
{
	class GdiplusEnv
	{
	public:
		explicit GdiplusEnv()noexcept;
		~GdiplusEnv() noexcept;

	private:
		struct GdiplusEnvImpl;
		std::unique_ptr<GdiplusEnvImpl> impl_;
	};

}