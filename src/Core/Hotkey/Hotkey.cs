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
            get => this.key;
            set
            {
                if (value != Keys.ControlKey && value != Keys.Alt && value != Keys.Menu && value != Keys.ShiftKey)
                {
                    this.key = value;
                }
                else
                {
                    this.key = Keys.None;
                }
            }
        }

        public ModifierKeys ModifierKey
        {
            get
            {
                return (this.Windows ? ModifierKeys.Win : ModifierKeys.None) |
(this.Control ? ModifierKeys.Control : ModifierKeys.None) |
(this.Shift ? ModifierKeys.Shift : ModifierKeys.None) |
(this.Alt ? ModifierKeys.Alt : ModifierKeys.None);
            }
        }

        public Hotkey()
        {
            this.Reset();
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
                        this.Windows = true;
                    }
                    else if (k == "ctrl")
                    {
                        this.Control = true;
                    }
                    else if (k == "shift")
                    {
                        this.Shift = true;
                    }
                    else if (k == "alt")
                    {
                        this.Alt = true;
                    }
                    else
                    {
                        this.Key = (Keys)Enum.Parse(typeof(Keys), keyStr);
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
            if (this.Key != Keys.None)
            {
                stringBuilder
                    .AppendIf(this.Windows, "win + ")
                    .AppendIf(this.Control, "ctrl + ")
                    .AppendIf(this.Shift, "shift + ")
                    .AppendIf(this.Alt, "alt + ")
                    .Append(this.Key);
            }
            return stringBuilder.ToString();
        }

        public void Reset()
        {
            this.Alt = false;
            this.Control = false;
            this.Shift = false;
            this.Windows = false;
            this.Key = Keys.None;
        }
    }
}
