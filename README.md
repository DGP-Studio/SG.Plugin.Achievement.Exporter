# Snap Genshin 插件-成就识别

## 说明

> 原神成就识别，主要用于快速查找未完成的隐藏成就，目前只支持“天地万象”的成就导出。
>
> - 支持任意分辨率下的窗口化原神。
> - 仅支持中文识别，准确率约 80%，分辨率越高识别越准确。

本插件基于[Snap Genshin](https://github.com/DGP-Studio/Snap.Genshin)的插件模板进行开发。

安装方法：将`dll`放入SG下的Plugins目录。

> 解决方案移植自：[genshin-achievement-toy](https://github.com/babalae/genshin-achievement-toy)

## 下载
[下载页](https://github.com/emako/SG.Plugin.Drop.Wish/releases/latest)

## 使用方法

1. 首先打开原神，并设置成窗口化，打开“天地万象”成就页面。
2. 点击“开始识别”，在弹出框中确认识别选区没有问题后，然后点击“确定”，程序会自动滚动成就页面进行截图识别。**此时不要移动鼠标，等成就识别完成即可，如果出现异常情况可以按F11停止识别**。
3. 识别完成后，可以选择以下网站进行数据导入并查看，具体导入方式可以看注释。
   - [cocogoat.work](https://cocogoat.work/)
   - [seelie.me](https://seelie.me/)
   - [paimon.moe](https://paimon.moe/)

## FAQ

- 为什么需要管理员权限？
  - 因为游戏以管理员权限启动，软件不以管理员权限启动的话没法模拟鼠标点击与滚动。
- ~~如何修改停止快捷键（F11）以及其他程序参数？~~
  - ~~软件同级目录下的 `config.json` 可以修改相关参数。~~
- 多次点击识别时报错？
  - 重启软件，建议每次识别成就都重启下软件。

