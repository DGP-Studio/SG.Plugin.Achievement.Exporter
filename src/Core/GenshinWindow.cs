using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin
{
    public class GenshinWindow
    {
        public static int WM_MOUSEWHEEL = 0x020A; // 滚轮滑动
        public static int WM_MOUSEMOVE = 0x200; // 鼠标移动
        public static int WM_LBUTTONDOWN = 0x201; //按下鼠标左键
        public static int WM_LBUTTONUP = 0x202; //释放鼠标左键

        public IntPtr HWND { get; set; }

        public GenshinWindow()
        {
        }

        internal static IntPtr MAKEWPARAM(int direction, float multiplier, WinMsgMouseKey button)
        {
            int delta = (int)(SystemInformation.MouseWheelScrollDelta * multiplier);
            return (IntPtr)((delta << 16) * Math.Sign(direction) | (ushort)button);
        }

        internal static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)((high << 16) | (low & 0xFFFF));
        }

        public bool FindWindowHandle()
        {
            Process[] pros = Process.GetProcessesByName("YuanShen");
            if (pros.Any())
            {
                HWND = pros[0].MainWindowHandle;
                return true;
            }
            else
            {
                pros = Process.GetProcessesByName("GenshinImpact");
                if (pros.Any())
                {
                    HWND = pros[0].MainWindowHandle;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Focus()
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_RESTORE = 0xF120;
            NativeMethod.SendMessage(HWND, WM_SYSCOMMAND, SC_RESTORE, 0);
            NativeMethod.SetForegroundWindow(HWND);
            while (NativeMethod.IsIconic(HWND))
                continue;
        }

        public Rectangle GetSize()
        {
            NativeMethod.RECT rc = new();
            NativeMethod.GetWindowRect(HWND, ref rc);
            return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        public void MouseWheelUp()
        {
            NativeMethod.MouseEvent(NativeMethod.MouseEventFlag.Wheel, 0, 0, 120, 0);
        }

        public void MouseWheelDown()
        {
            NativeMethod.MouseEvent(NativeMethod.MouseEventFlag.Wheel, 0, 0, -120, 0);
        }

        public void MouseMove(int x, int y)
        {
            NativeMethod.MouseEvent(NativeMethod.MouseEventFlag.Absolute | NativeMethod.MouseEventFlag.Move, 
                x * 65536 / PrimaryScreen.DESKTOP.Width, y * 65536 / PrimaryScreen.DESKTOP.Height,
                0, 0);
        }

        public void MoveCursor(int x, int y)
        {
            NativeMethod.SetCursorPos(x, y);
        }

        public void MouseLeftDown()
        {
            NativeMethod.MouseEvent(NativeMethod.MouseEventFlag.LeftDown, 0, 0, 0, 0);
        }

        public void MouseLeftUp()
        {
            NativeMethod.MouseEvent(NativeMethod.MouseEventFlag.LeftUp, 0, 0, 0, 0);
        }

        public void MouseClick(int x, int y)
        {
            int p = (y << 16) | x;
            NativeMethod.PostMessage(HWND, WM_LBUTTONDOWN, 0, p);
            Thread.Sleep(100);
            NativeMethod.PostMessage(HWND, WM_LBUTTONUP, 0, p);
        }

        public void Click(int x, int y, int h)
        {
            MouseMove(x, y + h / 2);
            MouseLeftDown();
            MouseLeftUp();
        }

        [Flags]
        public enum WinMsgMouseKey : int
        {
            MK_NONE = 0x0000,
            MK_LBUTTON = 0x0001,
            MK_RBUTTON = 0x0002,
            MK_SHIFT = 0x0004,
            MK_CONTROL = 0x0008,
            MK_MBUTTON = 0x0010,
            MK_XBUTTON1 = 0x0020,
            MK_XBUTTON2 = 0x0040
        }
    }
}
