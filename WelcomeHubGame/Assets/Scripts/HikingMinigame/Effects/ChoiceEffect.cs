using System;

namespace HikingMinigame.Effects
{
    [Serializable]
    public abstract class ChoiceEffect
    {
        public abstract void Apply(EffectContext context);
    }
}
