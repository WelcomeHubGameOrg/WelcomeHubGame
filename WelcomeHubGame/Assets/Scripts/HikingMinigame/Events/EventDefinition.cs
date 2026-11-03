using System;
using UnityEngine;

namespace HikingMinigame.Events
{
    public class EventDefinition : ScriptableObject
    {
        public const int UnlimitedOccurrences = -1;

        [field: SerializeField] public string Id { get; internal set; }
        [field: SerializeField] public string Title { get; internal set; }
        [field: SerializeField] public Sprite Icon { get; internal set; }
        [field: SerializeField, TextArea] public string Description { get; internal set; }
        [field: SerializeField] public float Weight { get; internal set; } = 1f;
        [field: SerializeField] public int MaxOccurrences { get; internal set; } = UnlimitedOccurrences;
        [field: SerializeField] public EventTrigger Trigger { get; internal set; }
        [field: SerializeField] public EventChoice[] Choices { get; internal set; } = Array.Empty<EventChoice>();

        public bool HasOccurrenceLimit => MaxOccurrences != UnlimitedOccurrences;
    }
}
