#pragma once

#include <memory>
#include <string_view>

namespace toes::imaging
{
    class Screenshot
    {
        struct Impl;
        std::unique_ptr<Impl> pImpl;

    public:
        Screenshot();
        Screenshot(int x, int y, int width, int height);
        ~Screenshot(); // 必须显式声明，以便 unique_ptr 能调用 Impl 析构

    public:
        void saveAs(std::string_view filename);

        // 禁止拷贝（pImpl 为 unique_ptr）
        Screenshot(const Screenshot&) = delete;
        Screenshot& operator=(const Screenshot&) = delete;
        // 允许移动
        Screenshot(Screenshot&&) noexcept = default;
        Screenshot& operator=(Screenshot&&) noexcept = default;
    };
}