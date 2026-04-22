#pragma once

#include "../Win32Utils.h"
#include "./WindowComposition.h"

namespace Win32Namespace
{
    // 任务栏
    class TaskbarCtrl
    {
        using ACCENT_STATE = WindowComposition::ACCENT_STATE;

    public:
        explicit TaskbarCtrl(COLORREF bgColor = 0x7a7ae9, BYTE nOpac = 68, BYTE nOpacAll = 255);

        TaskbarCtrl(const TaskbarCtrl&) = delete;
        ~TaskbarCtrl() = default;

    public:
        BOOL update() const;
        void OnTaskbarCreated();

    private:
        COLORREF m_crAlphaColor; // 颜色
        BYTE m_dwOpacity;        // 颜色透明度 0-255
        ACCENT_STATE m_style;    // 样式
        BYTE m_dwOpacityAll;     // 整体透明度 0-255
        HWND m_hWnd;
    };
}