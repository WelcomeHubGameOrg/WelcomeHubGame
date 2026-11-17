using System;
using System.Linq;
using UnityEngine;

namespace HikingMinigame.Items
{
    public class ItemDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; internal set; }
        [field: SerializeField] public string DisplayName { get; internal set; }
        [field: SerializeField] public Sprite Icon { get; internal set; }
        [field: SerializeField, TextArea] public string Description { get; internal set; }
        [field: SerializeField] public HazardType[] ProtectsAgainst { get; internal set; } = Array.Empty<HazardType>();
        [field: SerializeField] public int ReviveStamina { get; internal set; }
        [field: SerializeField] public bool SingleUse { get; internal set; }

        public bool CanRevive => ReviveStamina > 0;

        public bool Protects(HazardType hazard) => hazard != HazardType.None && ProtectsAgainst.Contains(hazard);
    }
}
