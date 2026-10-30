#include "GdiplusEnv.h"

#include <gdiplus.h>

#pragma comment(lib, "Gdiplus.lib")

struct Win32Namespace::GdiplusEnv::GdiplusEnvImpl
{
	ULONG_PTR token;
	Gdiplus::GdiplusStartupInput in;
};

Win32Namespace::GdiplusEnv::GdiplusEnv()
{
	impl_ = std::make_unique<GdiplusEnvImpl>();

	auto const status = Gdiplus::GdiplusStartup(&impl_->token, &impl_->in, nullptr);
	if (status != Gdiplus::Ok) {
		impl_.reset();	// 启动失败就不该在析构时对无效 token 调用 GdiplusShutdown
	}
}

Win32Namespace::GdiplusEnv::~GdiplusEnv() noexcept
{
	if (impl_) {
		Gdiplus::GdiplusShutdown(impl_->token);
	}
}
