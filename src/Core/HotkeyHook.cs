using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin
{
    /// <summary>
    /// Code from:
    /// https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp
    /// </summary>
    public sealed class HotkeyHook : IDisposable
    {
        private class Window : NativeWindow, IDisposable
        {
            private static readonly int WM_HOTKEY = 0x0312;

            public event EventHandler<KeyPressedEventArgs>? KeyPressed;

            public Window()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public void Dispose()
            {
                DestroyHandle();
            }
        }

        private Window window;
        private int currentId;

        public HotkeyHook()
        {
            window = new();
            window.KeyPressed += (_, e) => KeyPressed?.Invoke(this, e);
        }

        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            currentId += 1;
            if (!NativeMethod.RegisterHotKey(window.Handle, currentId, (uint)modifier, (uint)key))
            {
                if (Marshal.GetLastWin32Error() == 1409)
                    throw new InvalidOperationException("热键已经被占用");
                else
                    throw new InvalidOperationException("热键注册失败");
            }
        }

        public void UnregisterHotKey()
        {
            for (int i = currentId; i > 0; i--)
            {
                NativeMethod.UnregisterHotKey(window.Handle, i);
            }
        }

        public event EventHandler<KeyPressedEventArgs>? KeyPressed;

        public void Dispose()
        {
            UnregisterHotKey();
            window?.Dispose();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public ModifierKeys Modifier { get; }

        public Keys Key { get; }

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
