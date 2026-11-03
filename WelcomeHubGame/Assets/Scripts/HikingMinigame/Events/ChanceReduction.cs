using System;
using HikingMinigame.Items;
using UnityEngine;

namespace HikingMinigame.Events
{
    [Serializable]
    public sealed class ChanceReduction
    {
        [field: SerializeField] public ItemDefinition Item { get; internal set; }
        [field: SerializeField, Range(0f, 1f)] public float Amount { get; internal set; }
    }
}
