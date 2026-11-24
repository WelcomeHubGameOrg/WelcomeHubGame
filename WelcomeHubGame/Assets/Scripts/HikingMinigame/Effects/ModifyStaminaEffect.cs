using System;
using HikingMinigame.Items;
using UnityEngine;

namespace HikingMinigame.Effects
{
    [Serializable]
    public sealed class ModifyStaminaEffect : ChoiceEffect
    {
        [field: SerializeField] public int Amount { get; internal set; }
        [field: SerializeField] public HazardType Hazard { get; internal set; }

        public override void Apply(EffectContext context)
        {
            var change = context.Game.ChangeStamina(Amount, Hazard);

            if (change.ProtectedBy != null)
                context.Report($"Your {change.ProtectedBy.DisplayName} protected you.");
            else if (change.Delta != 0)
                context.Report($"Stamina {change.Delta:+#;-#}");

            if (change.RevivedBy != null)
                context.Report($"Your {change.RevivedBy.DisplayName} got you back on your feet.");
        }
    }
}
