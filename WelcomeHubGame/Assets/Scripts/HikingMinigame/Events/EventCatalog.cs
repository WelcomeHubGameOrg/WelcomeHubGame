using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HikingMinigame.Events
{
    [CreateAssetMenu(fileName = "EventCatalog", menuName = "Hiking/Event Catalog")]
    public class EventCatalog : ScriptableObject
    {
        [field: SerializeField] public EventDefinition[] Events { get; internal set; } = Array.Empty<EventDefinition>();

        public EventDefinition StartEvent => Events.FirstOrDefault(definition => definition.Trigger == EventTrigger.Start);

        public IEnumerable<EventDefinition> RandomEvents =>
            Events.Where(definition => definition.Trigger == EventTrigger.Random);
    }
}
