using System;
using HikingMinigame.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HikingMinigame.Events
{
    [Serializable]
    public sealed class EventChoice
    {
        [field: SerializeField] public string Text { get; internal set; }
        [field: SerializeField] public ItemDefinition RequiredItem { get; internal set; }
        [field: SerializeField] public Outcome Outcome { get; internal set; } = new();
        [field: SerializeField] public Risk Risk { get; internal set; } = new();

        public bool IsAvailable(Inventory inventory) => RequiredItem == null || inventory.Has(RequiredItem);

        public Outcome Resolve(Inventory inventory) =>
            Random.value < Risk.ChanceFor(inventory) ? Risk.Outcome : Outcome;
    }
}
