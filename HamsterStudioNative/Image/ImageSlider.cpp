
#include "./ImageSlider.h"

#include <opencv2/opencv.hpp>

#include <filesystem>
#include <random>

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4100d.lib")
#else
#pragma comment(lib, "opencv_world4100.lib")
#endif // _DEBUG

#pragma warning(push)
#pragma warning(disable : 4244)

HamsterStudio::ImageSlider::ImageSlider(std::string_view windowName, int cw, int ch)
{
    client_width = cw;
    client_height = ch;
    cd = 1.0 * cw / ch;

    svTitle = windowName;
    cv::namedWindow(svTitle);
    cv::moveWindow(svTitle, 10, 10);
}

int HamsterStudio::ImageSlider::ShowImage(std::string const &filename)
{
    static cv::Mat temp;
    static const auto cur_dir_len = std::filesystem::current_path().string().length();

    auto img = cv::imread(filename);
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

int HamsterStudio::ImageSlider::run(int delay)
{
    using namespace std;
    try {
        // auto args = tos::parser::CmdLineArgs::getInst();
        // playing_delay = args.has_key("delay") ? util::stoi(args["delay"]) : args.has_key("d") ? util::stoi(args["d"])
        //                                                                                       : delay;
        // bool recur = args.has_key("recur");
        // bool rand_play = args.has_key("rand");

        // cout << (recur ? "" : "非") << "递归读取" << endl;
        // cout << (rand_play ? "" : "非") << "随机播放" << endl;
        // cout << "播放间隔:" << (delay > 0 ? to_string(delay) : "随机") << endl;

        // vector<string> files = util::fs::GetFiles(filesystem::current_path(), util::ImageFilterOpenCV, recur);
        // cout << "共计读取 " << files.size() << " 个文件." << endl;

        while (true) {
            // if (rand_play)
            //{
            //     shuffle(files.begin(), files.end(), random_device{});
            // }

            // for (auto const &file : files)
            //{
            //     cout << file;
            //     int delay = ShowImage(file);
            //     cv::waitKey(delay);
            //     cout << "  delay:" << delay << "ms" << endl;
            // }
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
