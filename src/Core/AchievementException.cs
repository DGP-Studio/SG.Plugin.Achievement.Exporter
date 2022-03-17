using System;

namespace Achievement.Exporter.Plugin.Core
{
    internal class AchievementException : Exception
    {
        public AchievementException(string? message, Exception exception) : base(message, exception)
        {
        }
    }
}
