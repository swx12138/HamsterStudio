using System.Runtime.InteropServices;

namespace HamsterStudio.Toolkits.PInvoke
{
    public class DesktopHelper
    {
        private const int SMTO_NORMAL = 0x0;
        private const int WM_SHELLHOOK = 0x004D;
        private const int WM_USER = 0x0400;
        private const int WM_SHELLHOOKID_BROADCAST = WM_USER + 20;
        private const int HWND_BROADCAST = 0xFFFF;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint FindWindowEx(nint hwndParent, nint hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SendMessageTimeout(nint hWnd, int msg, int wParam, int lParam, int fuFlags, int uTimeout, out int lpdwResult);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);

        public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        public static nint FindWallpaperLow(nint theDesktopWorkerW = default)
        {
            nint[] pParam = [nint.Zero, theDesktopWorkerW];

            int result;
            nint windowHandle = FindWindow("Progman", null);
            SendMessageTimeout(windowHandle, 0x052c, 0, 0, SMTO_NORMAL, 0x3e8, out result);

            EnumWindows(new EnumWindowsProc((tophandle, lParam) =>
            {
                nint defview = FindWindowEx(tophandle, nint.Zero, "SHELLDLL_DefView", null);
                if (defview != nint.Zero)
                {
                    pParam[0] = FindWindowEx(nint.Zero, tophandle, "WorkerW", null);
                    if (pParam[1] != nint.Zero)
                        pParam[1] = tophandle;
                    return false;
                }
                return true;
            }), nint.Zero);

            if (pParam[0] == nint.Zero)
            {
                return nint.Zero;
            }

            return pParam[0];
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint SetParent(nint hWndChild, nint hWndNewParent);
    }

}
