#pragma once

#include <memory>
#include <string>

#define ImageEditorNamespace HamsterStudio::ImageEditor

namespace ImageEditorNamespace
{
	class PixelData {
		using DataTye = double;
	public:
		double Red, Green, Blue;
	public:
		explicit PixelData() noexcept;
		explicit PixelData(DataTye r = 0, DataTye g = 0, DataTye b = 0) noexcept;
		virtual ~PixelData() noexcept;
	};

	class PixelDataProcessor {
	public:
		using Pointer = std::shared_ptr<PixelDataProcessor>;

		virtual ~PixelDataProcessor() noexcept;
		virtual PixelData Process(PixelData &) const;

	private:
		Pointer nextProcessor_;
	};

	class DirectPixelDataProcessor :public PixelDataProcessor {
		double r_, g_, b_;
	public:
		explicit DirectPixelDataProcessor(double r, double g, double b) noexcept;
		virtual ~DirectPixelDataProcessor() noexcept;
		virtual PixelData Process(PixelData & pixel) const;
	};

	class ImageEditor
	{
		class ImageData;
		using ImageDataPtr = std::shared_ptr<ImageData>;
	public:
		explicit ImageEditor() noexcept;
		virtual	~ImageEditor() noexcept;

		bool LoadImage(std::string_view filename);
		bool SaveImage(std::string_view filename);

		void SetPixelDataProcessor(PixelDataProcessor::Pointer processor) noexcept;
		void ApplyPixelDataProcessor() noexcept;

	protected:
		ImageDataPtr imageData_;
		PixelDataProcessor::Pointer pixelDataProcessor_;
	};

}
