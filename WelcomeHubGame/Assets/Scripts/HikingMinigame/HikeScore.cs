using System;

namespace HikingMinigame
{
    public readonly struct HikeScore
    {
        public HikeScore(TimeSpan duration, int timePoints, int naturePoints, bool isWin)
        {
            Duration = duration;
            TimePoints = timePoints;
            NaturePoints = naturePoints;
            IsWin = isWin;
        }

        public TimeSpan Duration { get; }
        public int TimePoints { get; }
        public int NaturePoints { get; }
        public int Total => TimePoints + NaturePoints;
        public bool IsWin { get; }
    }
}
