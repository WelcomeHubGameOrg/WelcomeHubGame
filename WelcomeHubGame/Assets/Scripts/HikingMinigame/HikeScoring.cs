using System;
using UnityEngine;

namespace HikingMinigame
{
    [Serializable]
    public sealed class HikeScoring
    {
        [field: SerializeField, Min(1f)] public float FullTimePointsSeconds { get; private set; } = 180f;
        [field: SerializeField, Min(0)] public int MaxTimePoints { get; private set; } = 500;
        [field: SerializeField, Min(0)] public int MaxNaturePoints { get; private set; } = 500;
        [field: SerializeField, Min(0)] public int PointsToWin { get; private set; } = 500;

        public HikeScore Calculate(float elapsedSeconds, int natureScore, int maxNatureScore)
        {
            var timePoints = Mathf.RoundToInt(MaxTimePoints * Mathf.Clamp01(elapsedSeconds / FullTimePointsSeconds));
            var naturePoints = Mathf.RoundToInt(MaxNaturePoints * Mathf.Clamp01((float)natureScore / maxNatureScore));
            var isWin = timePoints + naturePoints >= PointsToWin;

            return new HikeScore(TimeSpan.FromSeconds(elapsedSeconds), timePoints, naturePoints, isWin);
        }
    }
}
