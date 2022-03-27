using Achievement.Exporter.Plugin.Helper;
using Achievement.Exporter.Plugin.Model;
using Snap.Data.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace Achievement.Exporter.Plugin
{
    public class PaimonMoeJson
    {
        public Dictionary<string, List<ExistAchievement>>? All { get; set; }

        /// <summary>
        /// 按id索引
        /// </summary>
        public Dictionary<int, ExistAchievement>? AchievementDic { get; set; }

        public static PaimonMoeJson Build()
        {
            static byte[] GetBytes(string uriString)
            {
                Uri uri = new(uriString);
                StreamResourceInfo info = Application.GetResourceStream(uri);
                using BinaryReader stream = new(info.Stream);
                return stream.ReadBytes((int)info.Stream.Length);
            }

            PaimonMoeJson paimonMoe = new();
            string achievementJson = Encoding.UTF8.GetString(GetBytes("pack://application:,,,/Achievement.Exporter.Plugin;component/Resources/Achievement.json"));
            paimonMoe.All = Json.ToObjectOrNew<Dictionary<string, List<ExistAchievement>>>(achievementJson);
            return paimonMoe;
        }

        public ExistAchievement Matching(string edition, OcrAchievement achievement, bool onlyChinese = false)
        {
            if (string.IsNullOrEmpty(achievement.OcrText))
            {
                return null!;
            }

            double max = 0;
            ExistAchievement maxMatch = null!;
            foreach (ExistAchievement existAchievement in this.All![edition])
            {
                double n = this.Matching(achievement, existAchievement, onlyChinese);
                if (n > max)
                {
                    max = n;
                    maxMatch = existAchievement;
                }

            }
            if (max > 0.7 && maxMatch != null && !string.IsNullOrWhiteSpace(achievement.OcrText))
            {
                if (achievement.OcrText.Contains("达成"))
                {
                    achievement.Match = maxMatch;
                    maxMatch.ocrAchievement = achievement;
                    // 成就集合要再次匹配描述，并把下级成就给完成
                    if (maxMatch.levels != null && maxMatch.levels.Count > 1)
                    {
                        this.MatchingMutilLevels(achievement, maxMatch, this.List2Dic(this.All[edition]));
                    }
                    else
                    {
                        //if(max < 0.9)
                        //{
                        //    Console.WriteLine($"{ocrAchievement.OcrAchievementName + ocrAchievement.OcrAchievementDesc} 最大匹配 {maxMatch?.name + maxMatch?.desc} 匹配度 {max}");
                        //}
                        maxMatch.done = true;
                    }
                }
                else if (maxMatch.levels != null && maxMatch.levels.Count > 1)
                {
                    this.MatchingMutilLevels(achievement, maxMatch, this.List2Dic(this.All[edition]), false);
                }
            }
            else
            {
                if (achievement.OcrText.Contains("达成") && !onlyChinese)
                {
                    this.Matching(edition, achievement, true);
                }
                Trace.WriteLine($"{achievement.OcrLeftText} 最小匹配 {maxMatch?.name + maxMatch?.desc} 匹配度 {max}");
            }

            return maxMatch!;
        }

        public ExistAchievement Match(string category, OcrAchievement achievement, bool onlyChinese = false)
        {
            if (string.IsNullOrEmpty(achievement.OcrText))
            {
                return null!;
            }

            double max = 0;
            ExistAchievement maxMatch = null!;
            foreach (ExistAchievement existAchievement in this.All![category])
            {
                double n = this.Matching(achievement, existAchievement, onlyChinese);

                if (n > max)
                {
                    max = n;
                    maxMatch = existAchievement;
                }
            }
            if (max > 0.7 && maxMatch != null)
            {
                if (!string.IsNullOrWhiteSpace(achievement.OcrText) && achievement.OcrText.Contains("达成"))
                {
                    achievement.Match = maxMatch;
                    maxMatch.ocrAchievement = achievement;
                    // 成就集合要再次匹配描述，并把下级成就给完成
                    if (maxMatch.levels?.Count > 1)
                    {
                        this.MatchingMutilLevels(achievement, maxMatch, this.List2Dic(this.All[category]));
                    }
                    else
                    {
                        maxMatch.done = true;
                    }
                }
            }
            else
            {
                if (achievement.OcrText.Contains("达成") && !onlyChinese)
                {
                    this.Matching(category, achievement, true);
                }
                Trace.WriteLine($"{achievement.OcrLeftText} 最小匹配 {maxMatch?.name + maxMatch?.desc} 匹配度 {max}");
            }

            return maxMatch!;
        }

        private double Matching(OcrAchievement ocr, ExistAchievement exist, bool onlyChinese)
        {
            if (string.IsNullOrEmpty(ocr.OcrLeftText))
            {
                return -1;
            }
            if (onlyChinese)
            {
                return TextHelper.Similarity(TextHelper.RetainChineseString(ocr.OcrLeftText), TextHelper.RetainChineseString(exist.name + exist.desc));
            }
            else
            {
                return TextHelper.Similarity(ocr.OcrLeftText, exist.name + exist.desc);
            }
        }

        /// <summary>
        /// 天地万象总共就4中多等级的
        /// </summary>
        private void MatchingMutilLevels(OcrAchievement ocr, ExistAchievement exist, Dictionary<int, ExistAchievement> dic, bool done = true)
        {
            if (exist.id == 80127 || exist.id == 80128 || exist.id == 80129)
            {
                if (exist.id == 80129)
                {
                    dic[80129].done = done;
                    dic[80128].done = true;
                    dic[80127].done = true;
                }
                else if (exist.id == 80128)
                {
                    dic[80128].done = done;
                    dic[80127].done = true;
                }
                else if (exist.id == 80127)
                {
                    dic[80127].done = done;
                }
            }
            else if (exist.id == 81026 || exist.id == 81027 || exist.id == 81028)
            {
                if (!string.IsNullOrEmpty(ocr.OcrLeftText))
                {
                    if (ocr.OcrLeftText.Contains('3'))
                    {
                        dic[81028].done = done;
                        dic[81027].done = true;
                        dic[81026].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains('2'))
                    {
                        dic[81027].done = done;
                        dic[81026].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains('1'))
                    {
                        dic[81026].done = done;
                    }
                }
            }
            else if (exist.id == 81029 || exist.id == 81030 || exist.id == 81031)
            {
                if (!string.IsNullOrEmpty(ocr.OcrLeftText))
                {
                    if (ocr.OcrLeftText.Contains('3'))
                    {
                        dic[81031].done = done;
                        dic[81030].done = true;
                        dic[81029].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains('2'))
                    {
                        dic[81030].done = done;
                        dic[81029].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains('1'))
                    {
                        dic[81029].done = done;
                    }
                }
            }
            else if (exist.id == 82041 || exist.id == 82042 || exist.id == 82043)
            {
                if (!string.IsNullOrEmpty(ocr.OcrLeftText))
                {
                    if (ocr.OcrLeftText.Contains("50000"))
                    {
                        dic[82043].done = done;
                        dic[82042].done = true;
                        dic[82041].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains("20000"))
                    {
                        dic[82042].done = done;
                        dic[82041].done = true;
                    }
                    else if (ocr.OcrLeftText.Contains("5000"))
                    {
                        dic[82041].done = done;
                    }
                }
            }
        }

        private Dictionary<int, ExistAchievement> List2Dic(List<ExistAchievement> list)
        {
            Dictionary<int, ExistAchievement> dic = new();

            foreach (ExistAchievement existAchievement in list)
            {
                dic.Add(existAchievement.id, existAchievement);
            }
            return dic;
        }
    }
}
