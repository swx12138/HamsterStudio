#include "../HamsterStudioNative/Application.h"

#define M_PI       3.14159265358979323846   // pi

static void PainFun(Window const& window, PAINTSTRUCT const& ps) {
    // 设置画笔
    HPEN hPen = CreatePen(PS_SOLID, 2, RGB(255, 0, 0));
    SelectObject(ps.hdc, hPen);

    // 假设我们要绘制正弦波
    auto size = window.GetSize();
    for (double x = 0; x < size.Width; x++) {
        double y = size.Height / 2 + (sin(x * M_PI / 180) * size.Height / 4); // 调整比例
        if (x > 0) {
            LineTo(ps.hdc, (int)x, (int)y);
        }
        else {
            MoveToEx(ps.hdc, (int)x, (int)y, NULL);
        }
    }

    DeleteObject(hPen);
}

int WINAPI WinMain(
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPSTR lpCmdLine,
	_In_ int nShowCmd
) {
	Application app{ hInstance, hPrevInstance, lpCmdLine, nShowCmd, "aaa" };

	Window::ProcedureContext proc{ PainFun };
	Window window{ app, proc, DefaultWindowSize, "bbb" };

	return window.Run();
}
