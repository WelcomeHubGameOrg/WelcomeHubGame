namespace HikingMinigame.Menu
{
    public static class HikeSession
    {
        public static string PlayerGuid { get; private set; }
        public static bool HasPlayer => !string.IsNullOrEmpty(PlayerGuid);
        public static bool HasWonAnyRun { get; private set; }
        public static bool StartImmediately { get; set; }

        public static void Begin(string playerGuid)
        {
            End();
            PlayerGuid = playerGuid;
        }

        public static void UsePlayer(string playerGuid) => PlayerGuid = playerGuid;

        public static void RecordRun(HikeScore score) => HasWonAnyRun |= score.IsWin;

        public static void End()
        {
            PlayerGuid = null;
            HasWonAnyRun = false;
            StartImmediately = false;
        }
    }
}
