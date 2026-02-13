#pragma once

#include "./PixelData/PixelDataProcessor.h"

#include <string>

#define ImageEditorNamespace HamsterStudio::ImageEditor

namespace ImageEditorNamespace
{
	class ImageEditor
	{
		class ImageData;
		using ImageDataPtr = std::shared_ptr<ImageData>;
		using PixelDataProcessor = PixelData::Processors::PixelDataProcessor;

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
