#include "GdiplusEnv.h"

#include <gdiplus.h>

#pragma comment(lib, "Gdiplus.lib")

struct Win32Namespace::GdiplusEnv::GdiplusEnvImpl
{
	ULONG_PTR token;
	Gdiplus::GdiplusStartupInput in;
};

Win32Namespace::GdiplusEnv::GdiplusEnv() noexcept
{
	auto status = Gdiplus::GdiplusStartup(&impl_->token, &impl_->in, nullptr);

}

Win32Namespace::GdiplusEnv::~GdiplusEnv() noexcept
{
	Gdiplus::GdiplusShutdown(impl_->token);
}
