using System;
using UnityEngine;

namespace HikingMinigame.Effects
{
    [Serializable]
    public sealed class ModifyNatureScoreEffect : ChoiceEffect
    {
        [field: SerializeField] public int Amount { get; internal set; }

        public override void Apply(EffectContext context)
        {
            var delta = context.Game.ChangeNatureScore(Amount);

            if (delta != 0)
                context.Report($"Nature score {delta:+#;-#}");
        }
    }
}
