
#include "./ImageSlider.h"

#include <opencv2/opencv.hpp>

#include <algorithm>
#include <filesystem>
#include <random>
#include <vector>

#include "./Tools/ImageUtils.hpp"

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4120d.lib")
#else
#pragma comment(lib, "opencv_world4120.lib")
#endif // _DEBUG

#pragma warning(push)
#pragma warning(disable : 4244)

HamsterStudioToes::ImageSlider::ImageSlider(std::string_view windowName, int cw, int ch)
{
    client_width = cw;
    client_height = ch;
    cd = 1.0 * cw / ch;

    svTitle = windowName;
    cv::namedWindow(svTitle);
    cv::moveWindow(svTitle, 10, 10);
}

int HamsterStudioToes::ImageSlider::ShowImage(std::string const &filename)
{
    static cv::Mat temp;
    static const auto cur_dir_len = std::filesystem::current_path().string().length();

    auto img = cv::imread(filename);
    if (img.empty()) {
        return 1;   // 读到损坏文件或非图片时直接跳过，避免用空 Mat 继续算尺寸
    }

    temp = cv::Mat::zeros(client_height, client_width, img.type());

    // 扁度
    auto d = 1.0 * img.cols / img.rows;
    int nw = client_width, nh = client_height;
    if (d > cd) {
        nh = nw / d;
    } // 图片比窗口更扁
    else {
        nw = nh * d;
    }

    auto roi = temp(cv::Rect((client_width - nw) / 2, (client_height - nh) / 2, nw, nh));
    cv::resize(img, roi, cv::Size(nw, nh));
    // cv::resize(img, roi, cv::Size(nw, nh));

    cv::putText(
        temp,
        std::filesystem::path(filename).filename().string(),
        cv::Point(0, 30), 0, 0.8,
        cv::Scalar(0, 255, 0));

    cv::imshow(svTitle, temp);

    return playing_delay > 0 ? playing_delay : std::random_device {}() % 3000 + 1500;
}

int HamsterStudioToes::ImageSlider::run(int delay)
{
    namespace fs = std::filesystem;
    using namespace std;

    playing_delay = delay;

    // 收集当前目录下的图片。原来这里是一个空的 while (true)，调用即满核空转、永不返回。
    vector<string> files;
    {
        error_code ec;
        for (fs::directory_iterator it{ fs::current_path(), fs::directory_options::skip_permission_denied, ec }, end;
             !ec && it != end; it.increment(ec)) {
            if (!it->is_regular_file(ec)) {
                continue;
            }
            if (auto const &path = it->path(); toes::IsSupportedExtension(path)) {
                files.push_back(path.string());
            }
        }
    }

    if (files.empty()) {
        return 0;
    }
    sort(files.begin(), files.end());

    try {
        for (auto const &file : files) {
            int const wait = ShowImage(file);
            if (cv::waitKey(wait) == 27) {          // ESC 退出
                break;
            }
            if (cv::getWindowProperty(svTitle, cv::WND_PROP_VISIBLE) < 1) {
                break;                              // 窗口已被用户关闭
            }
        }
    }
    catch (cv::Exception &ex) {
        cout << "cv::Exception\n\t" << ex.what() << endl;
    }
    catch (exception &ex) {
        cout << ex.what() << endl;
    }
    catch (...) {
        cout << "未知异常" << endl;
    }

    return 0;
}

#pragma warning(pop)
