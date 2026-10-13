using Effect;
using UnityEngine;

namespace HikingMinigame.Effect
{
    [CreateAssetMenu(fileName = "ModifyNatureScore", menuName = "Effects/ModifyNatureScore")]
    public class ModifyNatureScoreEffect : ChoiceEffect
    {
        public int amount;

        public override void Apply()
        {
            HikingGameManager.Instance.ModifyNatureScore(amount);
        }
    }
}
