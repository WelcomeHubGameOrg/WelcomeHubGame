namespace HikingMinigame.Menu
{
    public readonly struct HighscoreEntry
    {
        public HighscoreEntry(string playerName, int score, bool isCurrentPlayer)
        {
            PlayerName = playerName;
            Score = score;
            IsCurrentPlayer = isCurrentPlayer;
        }

        public string PlayerName { get; }
        public int Score { get; }
        public bool IsCurrentPlayer { get; }
    }
}
