using System;
using HikingMinigame.Items;
using UnityEngine;

namespace HikingMinigame.Effects
{
    [Serializable]
    public sealed class GiveItemEffect : ChoiceEffect
    {
        [field: SerializeField] public ItemDefinition Item { get; internal set; }

        public override void Apply(EffectContext context)
        {
            var message = context.Game.Inventory.TryAdd(Item)
                ? $"You got: {Item.DisplayName}."
                : $"Your bag is full, you leave the {Item.DisplayName} behind.";

            context.Report(message);
        }
    }
}
