namespace Achievement.Exporter.Plugin
{
    internal class AchievementMessage
    {
        public AchievementMessageLevel Level { get; set; } = AchievementMessageLevel.Info;
        public string? Message { get; set; }

        public AchievementMessage(string? message) : this(AchievementMessageLevel.Info, message)
        {
        }

        public AchievementMessage(AchievementMessageLevel level, string? message)
        {
            Level = level;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Level}]|{Message}";
        }
    }
}
