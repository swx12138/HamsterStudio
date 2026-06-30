#include <Image/ImageStitcher.h>

#include <Windows.h>

int main(int argc, char** argv)
{
    // 将控制台代码页设置为 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
    //ImageNamespace::Image::CheckCuda();
    return ImageNamespace::stitch_cli_main({ argv, argv + argc });
}
