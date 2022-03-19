using Achievement.Exporter.Plugin.View;
using DGP.Genshin.Core.Plugins;
using ModernWpf.Controls;
using System;

[assembly: SnapGenshinPlugin]

namespace Achievement.Exporter.Plugin
{
    [ImportPage(typeof(AchievementExporterPage), "成就识别", typeof(PluginIcon))]
    public class AchievementExporterPlugin : IPlugin
    {
        internal class PluginIcon : IconFactory
        {
            public override IconElement? GetIcon()
            {
                return new BitmapIcon()
                {
                    ShowAsMonochrome = true,
                    UriSource = new Uri("pack://application:,,,/Achievement.Exporter.Plugin;component/Resources/UI_Icon_Achievement.png"),
                };
            }
        }

        public string Name
        {
            get => "成就识别";
        }

        public string Description
        {
            get => "成就识别，快速查找未完成的隐藏成就，支持“天地万象”的成就导出，按F11停止。";
        }

        public string Author
        {
            get => "DGP Studio";
        }

        public Version Version
        {
            get => new("1.0.1");
        }

        public bool IsEnabled { get; set; }
    }
}
