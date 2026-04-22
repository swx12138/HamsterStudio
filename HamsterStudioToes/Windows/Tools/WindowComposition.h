#pragma once

#include "../Win32Utils.h"

namespace Win32Namespace {

    class WindowComposition
    {
    public:
        enum class Attribute : unsigned int
        {
            UNDEFINED = 0,
            NCRENDERING_ENABLED = 1,
            NCRENDERING_POLICY = 2,
            TRANSITIONS_FORCEDISABLED = 3,
            ALLOW_NCPAINT = 4,
            CAPTION_BUTTON_BOUNDS = 5,
            NONCLIENT_RTL_LAYOUT = 6,
            FORCE_ICONIC_REPRESENTATION = 7,
            EXTENDED_FRAME_BOUNDS = 8,
            HAS_ICONIC_BITMAP = 9,
            THEME_ATTRIBUTES = 10,
            NCRENDERING_EXILED = 11,
            NCADORNMENTINFO = 12,
            EXCLUDED_FROM_LIVEPREVIEW = 13,
            VIDEO_OVERLAY_ACTIVE = 14,
            FORCE_ACTIVEWINDOW_APPEARANCE = 15,
            DISALLOW_PEEK = 16,
            CLOAK = 17,
            CLOAKED = 18,
            ACCENT_POLICY = 19,
            FREEZE_REPRESENTATION = 20,
            EVER_UNCLOAKED = 21,
            VISUAL_OWNER = 22,
            LAST = 23
        };

        struct Data
        {
            Attribute nAttribute;
            PVOID pvData;
            SIZE_T cbData;
        };

        enum class ACCENT_STATE
        {
            DISABLED = 0,                   // 全黑
            ENABLE_GRADIENT = 1,            // 单色不透明
            ENABLE_TRANSPARENTGRADIENT = 2, // 全透明
            ENABLE_BLURBEHIND = 3,          // 模糊
            ENABLE_ACRYLICBLURBEHIND = 4,   // 亚克力
            INVALID_STATE = 5,              // 全黑
            ACCENT_NORMAL = 150             // (Fake value) Emulate regular taskbar appearance
        };

        struct AccentPolicy
        {
            ACCENT_STATE AccentState;
            DWORD AccentFlags;
            DWORD GradientColor;
            DWORD AnimationId;
        };

        static int Set(HWND hWnd, ACCENT_STATE mode, DWORD AlphaColor);
    };

}
