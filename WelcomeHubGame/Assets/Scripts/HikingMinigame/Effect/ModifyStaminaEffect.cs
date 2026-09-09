using Effect;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyStamina", menuName = "Effects/ModifyStamina")]
public class ModifyStaminaEffect : ChoiceEffect
{
    public int amount; // positive = restore, negative = drain

    public override void Apply()
    {
        // GameManager should own the actual stamina value and its clamping/max logic,
        // this effect just tells it what happened
        GameManager.Instance.ModifyStamina(amount);
    }
}