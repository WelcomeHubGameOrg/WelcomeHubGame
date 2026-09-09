using System.Collections.Generic;
using UnityEngine;

namespace Event
{
    [CreateAssetMenu(fileName = "EventCatalog", menuName = "Events/EventCatalog")]
    public class EventCatalog : ScriptableObject
    {
        public EventDefinition[] Events;

        private Dictionary<EventDefinition, int> _occurrenceCounts;

        private void ResetOccurrences()
        {
            _occurrenceCounts = new Dictionary<EventDefinition, int>();
        }

        public EventDefinition GetRandomEvent()
        {
            if (_occurrenceCounts == null)
                ResetOccurrences();

            var eligible = new List<EventDefinition>();
            foreach (var e in Events)
            {
                if (!e) continue;

                var count = _occurrenceCounts.GetValueOrDefault(e, 0);
                var underLimit = e.MaxOccurrences < 0 || count < e.MaxOccurrences;
                if (underLimit)
                    eligible.Add(e);
            }

            if (eligible.Count == 0)
                return null;

            var totalWeight = 0f;
            foreach (var e in eligible)
                totalWeight += e.Weight;

            var roll = Random.Range(0f, totalWeight);
            foreach (var e in eligible)
            {
                if (roll < e.Weight)
                {
                    _occurrenceCounts[e] = _occurrenceCounts.GetValueOrDefault(e, 0) + 1;
                    return e;
                }
                roll -= e.Weight;
            }

            return eligible[^1]; // fallback for float rounding edge cases
        }
    }
}