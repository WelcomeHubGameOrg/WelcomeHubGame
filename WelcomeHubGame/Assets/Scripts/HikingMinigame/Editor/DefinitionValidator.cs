using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HikingMinigame.Events;
using HikingMinigame.Items;
using UnityEditor;
using UnityEngine;

namespace HikingMinigame.Editor
{
    internal sealed class DefinitionValidator
    {
        private static readonly Regex IdPattern = new("^[a-z0-9_]+$");

        private readonly List<string> _errors = new();
        private readonly HashSet<string> _itemIds;

        private DefinitionValidator(IEnumerable<ItemYaml> items)
        {
            _itemIds = new HashSet<string>(items.Select(item => item.Id).Where(id => id != null));
        }

        public static IReadOnlyList<string> Validate(IReadOnlyList<ItemYaml> items, IReadOnlyList<EventYaml> events)
        {
            var validator = new DefinitionValidator(items);

            validator.CheckUniqueIds("item", items.Select(item => (item.SourceFile, item.Id)));
            validator.CheckUniqueIds("event", events.Select(definition => (definition.SourceFile, definition.Id)));

            foreach (var item in items)
                validator.ValidateItem(item);

            foreach (var definition in events)
                validator.ValidateEvent(definition);

            validator.CheckSingleStartEvent(events);
            return validator._errors;
        }

        private void CheckUniqueIds(string kind, IEnumerable<(string SourceFile, string Id)> entries)
        {
            var duplicates = entries
                .Where(entry => entry.Id != null)
                .GroupBy(entry => entry.Id)
                .Where(group => group.Count() > 1);

            foreach (var duplicate in duplicates)
            {
                var files = string.Join(", ", duplicate.Select(entry => entry.SourceFile));
                _errors.Add($"{kind} id '{duplicate.Key}' is used more than once ({files}).");
            }
        }

        private void CheckSingleStartEvent(IEnumerable<EventYaml> events)
        {
            var startEvents = events
                .Where(definition => YamlEnum.TryParse<EventTrigger>(definition.Trigger, out var trigger) && trigger == EventTrigger.Start)
                .Select(definition => definition.Id)
                .ToList();

            if (startEvents.Count > 1)
                _errors.Add($"Only one event may use 'trigger: start', found: {string.Join(", ", startEvents)}.");
        }

        private void ValidateItem(ItemYaml item)
        {
            var location = $"{item.SourceFile}: item '{item.Id}'";

            CheckId(location, item.Id);
            CheckRequired(location, "name", item.Name);
            CheckIcon(location, item.Icon);

            foreach (var hazard in item.ProtectsAgainst.OrEmpty())
                CheckHazard(location, hazard);

            if (item.ReviveStamina < 0)
                Fail(location, "reviveStamina must not be negative.");
        }

        private void ValidateEvent(EventYaml definition)
        {
            var location = $"{definition.SourceFile}: event '{definition.Id}'";

            CheckId(location, definition.Id);
            CheckRequired(location, "name", definition.Name);
            CheckIcon(location, definition.Icon);

            if (definition.Weight <= 0f)
                Fail(location, "weight must be greater than 0.");

            if (definition.MaxOccurrences < 1)
                Fail(location, "maxOccurrences must be at least 1, or left out for unlimited.");

            if (!YamlEnum.TryParse<EventTrigger>(definition.Trigger, out _))
                Fail(location, $"unknown trigger '{definition.Trigger}'. Use one of: {YamlEnum.Options<EventTrigger>()}.");

            var choices = definition.Choices.OrEmpty().ToList();
            if (choices.Count == 0)
                Fail(location, "needs at least one choice.");

            for (var i = 0; i < choices.Count; i++)
                ValidateChoice($"{location}, choice {i + 1}", choices[i]);
        }

        private void ValidateChoice(string location, ChoiceYaml choice)
        {
            CheckRequired(location, "text", choice.Text);

            if (choice.RequiresItem != null)
                CheckItemReference(location, "requiresItem", choice.RequiresItem);

            ValidateOutcome($"{location}, outcome", choice.Outcome);

            if (choice.Risk != null)
                ValidateRisk($"{location}, risk", choice.Risk);
        }

        private void ValidateRisk(string location, RiskYaml risk)
        {
            if (risk.Chance <= 0f || risk.Chance > 1f)
                Fail(location, "chance must be greater than 0 and at most 1.");

            foreach (var (itemId, amount) in risk.ReducedBy.OrEmpty())
            {
                CheckItemReference(location, "reducedBy", itemId);

                if (amount <= 0f || amount > 1f)
                    Fail(location, $"reducedBy '{itemId}' must be greater than 0 and at most 1.");
            }

            ValidateOutcome($"{location}, outcome", risk.Outcome);
        }

        private void ValidateOutcome(string location, OutcomeYaml outcome)
        {
            if (outcome == null)
            {
                Fail(location, "is missing.");
                return;
            }

            CheckRequired(location, "text", outcome.Text);

            var effects = outcome.Effects.OrEmpty().ToList();
            for (var i = 0; i < effects.Count; i++)
                ValidateEffect($"{location}, effect {i + 1}", effects[i]);
        }

        private void ValidateEffect(string location, EffectYaml effect)
        {
            if (effect.ActionCount != 1)
                Fail(location, "must contain exactly one of: stamina, nature, giveItem, removeItem.");

            if (effect.Hazard != null)
            {
                if (effect.Stamina == null)
                    Fail(location, "hazard can only be used together with stamina.");

                CheckHazard(location, effect.Hazard);
            }

            if (effect.GiveItem != null)
                CheckItemReference(location, "giveItem", effect.GiveItem);

            if (effect.RemoveItem != null)
                CheckItemReference(location, "removeItem", effect.RemoveItem);
        }

        private void CheckId(string location, string id)
        {
            if (id == null || !IdPattern.IsMatch(id))
                Fail(location, "id is required and may only contain a-z, 0-9 and _.");
        }

        private void CheckRequired(string location, string field, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                Fail(location, $"{field} is required.");
        }

        private void CheckIcon(string location, string icon)
        {
            if (string.IsNullOrEmpty(icon))
                return;

            if (AssetDatabase.LoadAssetAtPath<Sprite>(DefinitionPaths.Icon(icon)) == null)
                Fail(location, $"icon '{icon}' is not a sprite in {DefinitionPaths.IconFolder}.");
        }

        private void CheckHazard(string location, string hazard)
        {
            if (!YamlEnum.TryParse<HazardType>(hazard, out _))
                Fail(location, $"unknown hazard '{hazard}'. Use one of: {YamlEnum.Options<HazardType>()}.");
        }

        private void CheckItemReference(string location, string field, string itemId)
        {
            if (!_itemIds.Contains(itemId))
                Fail(location, $"{field} refers to unknown item '{itemId}'.");
        }

        private void Fail(string location, string message) => _errors.Add($"{location}: {message}");
    }
}
