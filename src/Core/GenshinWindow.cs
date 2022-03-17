using Snap.Win32;
using Snap.Win32.NativeMethod;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin.Core
{
    /// <summary>
    /// 表示原神窗口的抽象
    /// 定义对原神窗口的抽象操作
    /// </summary>
    public class GenshinWindow
    {
        private IntPtr HWND { get; set; }

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
            return (IntPtr)(high << 16 | low & 0xFFFF);
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
            _ = User32.SendMessage(HWND, WM_SYSCOMMAND, SC_RESTORE, 0);
            _ = User32.SetForegroundWindow(HWND);
            while (User32.IsIconic(HWND))
            {
                continue;
            }
        }

        public Rectangle GetSize()
        {
            RECT rc = new();
            User32.GetWindowRect(HWND, ref rc);
            return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        public void MouseWheelUp()
        {
            _ = User32.MouseEvent(User32.MouseEventFlag.Wheel, 0, 0, 120, 0);
        }

        /// <summary>
        /// 模拟向下滚动
        /// </summary>
        public void MouseWheelDown()
        {
            _ = User32.MouseEvent(User32.MouseEventFlag.Wheel, 0, 0, -120, 0);
        }

        public void MouseMove(int x, int y)
        {
            _ = User32.MouseEvent(User32.MouseEventFlag.Absolute | User32.MouseEventFlag.Move,
                x * 65536 / PrimaryScreen.DESKTOP.Width, y * 65536 / PrimaryScreen.DESKTOP.Height,
                0, 0);
        }

        public void MoveCursor(int x, int y)
        {
            User32.SetCursorPos(x, y);
        }

        public void MouseLeftDown()
        {
            _ = User32.MouseEvent(User32.MouseEventFlag.LeftDown, 0, 0, 0, 0);
        }

        public void MouseLeftUp()
        {
            _ = User32.MouseEvent(User32.MouseEventFlag.LeftUp, 0, 0, 0, 0);
        }

        public void MouseClick(int x, int y)
        {
            int p = y << 16 | x;
            User32.PostMessage(HWND, User32.WM_LBUTTONDOWN, 0, p);
            Thread.Sleep(100);
            User32.PostMessage(HWND, User32.WM_LBUTTONUP, 0, p);
        }

        /// <summary>
        /// 模拟点击
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="h"></param>
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
