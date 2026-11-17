using System;
using UnityEngine;

namespace HikingMinigame
{
    [Serializable]
    public sealed class StaminaDrainCurve
    {
        [field: SerializeField, Min(0f)] public float StartRatePerSecond { get; private set; } = 0.55f;
        [field: SerializeField, Min(1f)] public float DoublingSeconds { get; private set; } = 110f;

        public float RateAt(float elapsedSeconds) => StartRatePerSecond * Mathf.Pow(2f, elapsedSeconds / DoublingSeconds);
    }
}
