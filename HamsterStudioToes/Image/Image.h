#pragma once

#define ImageNamespace HamsterStudioToes::Image

#include <memory>
#include <filesystem>
#include <istream>

#include "../Shapes/Rectangle.h"
#include "../Export.h"

namespace ImageNamespace
{
	namespace fs = std::filesystem;

	struct ImageShape { int width, height, channels; };

#pragma warning(push)
#pragma warning(disable : 4251)	// pImpl 私有成员无需 dll 接口
	class TOES_API Image {
		struct Impl;
		std::shared_ptr<Impl> pImpl;

	public:
		explicit Image();	// 主要被委托使用，创建pImpl

		explicit Image(fs::path const& path, int opencvReadFlag = 1);
		explicit Image(std::istream& is, int opencvReadFlag = 1);

		Image(Image const& image) noexcept;
		Image(Image&& image) noexcept;

		Image& operator=(Image const& image) noexcept;
		Image& operator=(Image&& image) noexcept;

		Image operator()(Shapes::Rectangle const& rect) const;

	public:
		bool Valid() const;

		void* NativeMatPointer() const;
		static Image FromNativeMatPointer(void * pointer);
		static Image FromByteArray(std::vector<uint8_t> const& data, int flags);
		static Image FromByteArray(uint8_t* data, size_t dataLen, int flags);

		int Width() const;
		int Height() const;
		ImageShape GetShape() const;

		void ImShow(int width = 1600) const;
		void Save(fs::path const& filename, int quality = 95) const;
		void AdaptiveResize(int shortSide) const;

	public:
		static void CheckCuda();

	};
#pragma warning(pop)
}
