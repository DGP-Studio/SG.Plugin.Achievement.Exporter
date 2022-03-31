using System;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin.Core.Hotkey
{
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
}
