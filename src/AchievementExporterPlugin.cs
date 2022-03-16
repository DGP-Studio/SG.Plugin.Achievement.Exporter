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
            public override IconElement? GetIcon() => new BitmapIcon()
            {
                ShowAsMonochrome = true,
                UriSource = new Uri("pack://application:,,,/Achievement.Exporter.Plugin;component/Resources/UI_Icon_Achievement.png"),
            };
        }

        public string Name => "成就识别";
        public string Description => "本插件用于成就识别，主要用于快速查找未完成的隐藏成就，目前支持“天地万象”的成就导出。支持任意分辨率下的窗口化原神，仅支持中文识别准确率约80%，分辨率越高识别越准确。开始识别后不要移动鼠标，异常可按F11停止。";
        public string Author => "DGP Studio";
        public Version Version => new("1.0.0");
        public bool IsEnabled { get; set; }
    }
}
