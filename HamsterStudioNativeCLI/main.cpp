
#include "../HamsterStudioNative/Image/ImageEditor.h"

#include <chrono>
#include <iostream>

const std::string imagePath = "C:/Users/nv/Downloads/1167339074180087814_0_bili_f321850f1a39f3e229e5fa53eda782403546937055775240.jpg";
const std::string imageOutPath = "C:/Users/nv/Downloads/1167339074180087814_0_bili_f321850f1a39f3e229e5fa53eda782403546937055775240_.jpg";

int main()
{
	using namespace std::chrono;
	auto start = high_resolution_clock::now();

	ImageEditorNamespace::ImageEditor editor;
	editor.LoadImage(imagePath);

	ImageEditorNamespace::DirectPixelDataProcessor processor(0.5, 0.5, 0.5);
	editor.SetPixelDataProcessor(std::make_shared<ImageEditorNamespace::DirectPixelDataProcessor>(processor));
	editor.ApplyPixelDataProcessor();

	editor.SaveImage(imageOutPath);

	auto end = high_resolution_clock::now();
	auto duration = duration_cast<milliseconds>(end - start);
	std::cout << "Processing time: " << duration.count() << " ms" << std::endl;

	return 0;
}


