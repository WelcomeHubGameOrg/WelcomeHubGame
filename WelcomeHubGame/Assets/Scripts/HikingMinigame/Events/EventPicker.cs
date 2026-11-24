using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HikingMinigame.Events
{
    public sealed class EventPicker
    {
        private readonly IReadOnlyList<EventDefinition> _events;
        private readonly Dictionary<EventDefinition, int> _occurrences = new();

        public EventPicker(IEnumerable<EventDefinition> events)
        {
            _events = events.ToList();
        }

        public bool TryPick(out EventDefinition picked)
        {
            var eligible = _events.Where(IsUnderLimit).ToList();
            if (eligible.Count == 0)
            {
                picked = null;
                return false;
            }

            picked = PickWeighted(eligible);
            _occurrences[picked] = OccurrencesOf(picked) + 1;
            return true;
        }

        private bool IsUnderLimit(EventDefinition definition) =>
            !definition.HasOccurrenceLimit || OccurrencesOf(definition) < definition.MaxOccurrences;

        private int OccurrencesOf(EventDefinition definition) => _occurrences.GetValueOrDefault(definition);

        private static EventDefinition PickWeighted(IReadOnlyList<EventDefinition> candidates)
        {
            var roll = Random.Range(0f, candidates.Sum(candidate => candidate.Weight));
            foreach (var candidate in candidates)
            {
                if (roll < candidate.Weight)
                    return candidate;

                roll -= candidate.Weight;
            }

            return candidates[^1];
        }
    }
}
