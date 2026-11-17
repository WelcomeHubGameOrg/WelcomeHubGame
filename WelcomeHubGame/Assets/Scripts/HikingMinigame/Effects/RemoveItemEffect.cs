using System;
using HikingMinigame.Items;
using UnityEngine;

namespace HikingMinigame.Effects
{
    [Serializable]
    public sealed class RemoveItemEffect : ChoiceEffect
    {
        [field: SerializeField] public ItemDefinition Item { get; internal set; }

        public override void Apply(EffectContext context)
        {
            if (context.Game.Inventory.Remove(Item))
                context.Report($"You no longer have: {Item.DisplayName}.");
        }
    }
}
