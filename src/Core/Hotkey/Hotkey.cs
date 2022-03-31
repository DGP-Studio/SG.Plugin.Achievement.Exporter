using Snap.Data.Utility.Extension;
using System;
using System.Text;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin.Core.Hotkey
{
    /// <summary>
    /// 表示当前设置的热键
    /// </summary>
    public class Hotkey
    {
        public bool Alt { get; set; }
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Windows { get; set; }

        private Keys key;
        public Keys Key
        {
            get => key;
            set
            {
                if (value != Keys.ControlKey && value != Keys.Alt && value != Keys.Menu && value != Keys.ShiftKey)
                {
                    key = value;
                }
                else
                {
                    key = Keys.None;
                }
            }
        }

        public ModifierKeys ModifierKey
        {
            get
            {
                return (Windows ? ModifierKeys.Win : ModifierKeys.None) |
(Control ? ModifierKeys.Control : ModifierKeys.None) |
(Shift ? ModifierKeys.Shift : ModifierKeys.None) |
(Alt ? ModifierKeys.Alt : ModifierKeys.None);
            }
        }

        public Hotkey()
        {
            Reset();
        }

        public Hotkey(string hotkeyStr)
        {
            try
            {
                string[] keyStrs = hotkeyStr.Replace(" ", "").Split('+');
                foreach (string keyStr in keyStrs)
                {
                    string k = keyStr.ToLowerInvariant();
                    if (k == "win")
                    {
                        Windows = true;
                    }
                    else if (k == "ctrl")
                    {
                        Control = true;
                    }
                    else if (k == "shift")
                    {
                        Shift = true;
                    }
                    else if (k == "alt")
                    {
                        Alt = true;
                    }
                    else
                    {
                        Key = (Keys)Enum.Parse(typeof(Keys), keyStr);
                    }
                }
            }
            catch
            {
                throw new ArgumentException("无效的热键");
            }
        }

        public override string ToString()
        {
            string str = string.Empty;
            StringBuilder stringBuilder = new();
            if (Key != Keys.None)
            {
                stringBuilder
                    .AppendIf(Windows, "win + ")
                    .AppendIf(Control, "ctrl + ")
                    .AppendIf(Shift, "shift + ")
                    .AppendIf(Alt, "alt + ")
                    .Append(Key);
            }
            return stringBuilder.ToString();
        }

        public void Reset()
        {
            Alt = false;
            Control = false;
            Shift = false;
            Windows = false;
            Key = Keys.None;
        }
    }
}
