using System;
using System.Linq;
using HikingMinigame.Items;
using UnityEngine;

namespace HikingMinigame.Events
{
    [Serializable]
    public sealed class Risk
    {
        [field: SerializeField, Range(0f, 1f)] public float Chance { get; internal set; }
        [field: SerializeField] public ChanceReduction[] ReducedBy { get; internal set; } = Array.Empty<ChanceReduction>();
        [field: SerializeField] public Outcome Outcome { get; internal set; } = new();

        public float ChanceFor(Inventory inventory)
        {
            var totalReduction = ReducedBy
                .Where(entry => inventory.Has(entry.Item))
                .Sum(entry => entry.Amount);

            return Mathf.Clamp01(Chance - totalReduction);
        }
    }
}
