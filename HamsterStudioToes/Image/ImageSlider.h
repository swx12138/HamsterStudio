#pragma once

#include <string>

namespace HamsterStudio
{
    class ImageSlider
    {
    public:
        ImageSlider(std::string_view windowName = "slide", int cw = 650, int ch = 720);

        int ShowImage(std::string const &filename);

        int run(int delay = 500);

    private:
        int client_width;
        int client_height;
        std::string svTitle;
        int playing_delay;
        double cd;
    };

}