
#include <vcclr.h>

#include "./Unmanaged/Image/Tools/GradientFiller.h"
#include "./Unmanaged/Image/ImageStitcher.h"

#include <thread>
#include <future>

using namespace System;
using namespace System::Diagnostics;

namespace HamsterStudioBridge
{
	namespace OpenCvWrapper {

		// 1. 定义一个 C# 能看懂的结构体来代表 cv::Scalar
		// 这样 C# 调用时只需传入 4 个 double 或一个结构体
		public value struct ColorScalar
		{
			double Val0;
			double Val1;
			double Val2;
			double Val3;

			// 构造函数
			ColorScalar(double v0, double v1, double v2, double v3) {
				Val0 = v0; Val1 = v1; Val2 = v2; Val3 = v3;
			}

			static const ColorScalar Red{ 0, 0, 255,255 };
			static const ColorScalar Green{ 0, 255, 0, 255 };
			static const ColorScalar Blue{ 255, 0, 0, 255 };
			static const ColorScalar WhiteSmoke{ 245, 245, 245, 255 };

		};

		public value struct Size2d {
			int Width;
			int Height;
			
			Size2d(int w, int h)
				:Width(w),Height(h)
			{ }

			static const Size2d Empty{ 0,0 };
		};

		// 2. 创建包装类
		public ref class ImageProcessor
		{
		public:
			// 核心方法：供 C# 调用
			// matPtr: C# 传来的 cv::Mat 的内存地址指针
			// 颜色参数：使用上面定义的 ColorScalar
			static void FillBilinearWrapper(IntPtr matPtr, ColorScalar color_tl, ColorScalar color_tr, ColorScalar color_bl, ColorScalar color_br)
			{
				// 3. 将 IntPtr 还原为 C++ 指针
				// 假设 cv::Mat 是 64位对齐的，这里直接强制转换
				cv::Mat* nativeMat = reinterpret_cast<cv::Mat*>(matPtr.ToPointer());

				// 4. 将 C# 结构体转换为 C++ cv::Scalar
				cv::Scalar c_tl(color_tl.Val0, color_tl.Val1, color_tl.Val2, color_tl.Val3);
				cv::Scalar c_tr(color_tr.Val0, color_tr.Val1, color_tr.Val2, color_tr.Val3);
				cv::Scalar c_bl(color_bl.Val0, color_bl.Val1, color_bl.Val2, color_bl.Val3);
				cv::Scalar c_br(color_br.Val0, color_br.Val1, color_br.Val2, color_br.Val3);

				// 5. 调用原始 C++ 函数
				// 注意：这里直接传引用，因为 nativeMat 已经是 C++ 对象指针
				Image::Tools::GradientFiller::FillBilinear(*nativeMat, c_tl, c_tr, c_bl, c_br);
			}

			static bool GetStitchImages(array<IntPtr>^ inputs, IntPtr output, Size2d cellSize, int spacing)
			{
				std::vector<cv::Mat const*> mats{ static_cast<size_t>(inputs->Length) };
				for (int i = 0;i < inputs->Length;i++) {
					mats[i] = reinterpret_cast<cv::Mat*>(inputs[i].ToPointer());
				}

				auto mat = ImageStitcherNamespace::ImageStitcher::stitch(mats, cellSize.Width, cellSize.Height, spacing);

				auto pOutputMat = reinterpret_cast<cv::Mat*>(output.ToPointer());
				mat.copyTo(*pOutputMat);

				return true;
			}

		};
	}
}