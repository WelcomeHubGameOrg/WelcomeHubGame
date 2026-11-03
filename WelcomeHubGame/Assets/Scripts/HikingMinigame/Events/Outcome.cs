using System;
using HikingMinigame.Effects;
using UnityEngine;

namespace HikingMinigame.Events
{
    [Serializable]
    public sealed class Outcome
    {
        [field: SerializeField, TextArea] public string Text { get; internal set; }
        [field: SerializeReference] public ChoiceEffect[] Effects { get; internal set; } = Array.Empty<ChoiceEffect>();

        public void Apply(EffectContext context)
        {
            foreach (var effect in Effects)
                effect.Apply(context);
        }
    }
}
