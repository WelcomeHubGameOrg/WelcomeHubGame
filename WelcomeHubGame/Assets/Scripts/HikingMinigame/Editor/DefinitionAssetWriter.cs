using System.Collections.Generic;
using System.IO;
using System.Linq;
using HikingMinigame.Effects;
using HikingMinigame.Events;
using HikingMinigame.Items;
using UnityEditor;
using UnityEngine;

namespace HikingMinigame.Editor
{
    internal sealed class DefinitionAssetWriter
    {
        private readonly Dictionary<string, ItemDefinition> _itemsById = new();

        public void Write(IReadOnlyList<ItemYaml> items, IReadOnlyList<EventYaml> events)
        {
            EnsureFolder(DefinitionPaths.ItemFolder);
            EnsureFolder(DefinitionPaths.EventFolder);

            foreach (var item in items)
                _itemsById[item.Id] = WriteItem(item);

            var eventAssets = events
                .OrderBy(definition => definition.Id)
                .Select(WriteEvent)
                .ToArray();

            DeleteStaleAssets(DefinitionPaths.ItemFolder, items.Select(item => DefinitionPaths.Item(item.Id)));
            DeleteStaleAssets(DefinitionPaths.EventFolder, events.Select(definition => DefinitionPaths.Event(definition.Id)));
            UpdateCatalog(eventAssets);
        }

        private static ItemDefinition WriteItem(ItemYaml yaml)
        {
            var item = LoadOrCreate<ItemDefinition>(DefinitionPaths.Item(yaml.Id));

            item.Id = yaml.Id;
            item.DisplayName = yaml.Name;
            item.Icon = LoadIcon(yaml.Icon);
            item.Description = yaml.Description;
            item.ProtectsAgainst = yaml.ProtectsAgainst.OrEmpty().Select(YamlEnum.Parse<HazardType>).ToArray();
            item.ReviveStamina = yaml.ReviveStamina;
            item.SingleUse = yaml.SingleUse;

            EditorUtility.SetDirty(item);
            return item;
        }

        private EventDefinition WriteEvent(EventYaml yaml)
        {
            var definition = LoadOrCreate<EventDefinition>(DefinitionPaths.Event(yaml.Id));

            definition.Id = yaml.Id;
            definition.Title = yaml.Name;
            definition.Icon = LoadIcon(yaml.Icon);
            definition.Description = yaml.Description;
            definition.Weight = yaml.Weight;
            definition.MaxOccurrences = yaml.MaxOccurrences ?? EventDefinition.UnlimitedOccurrences;
            definition.Trigger = YamlEnum.Parse<EventTrigger>(yaml.Trigger);
            definition.Choices = yaml.Choices.Select(BuildChoice).ToArray();

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private EventChoice BuildChoice(ChoiceYaml yaml) => new()
        {
            Text = yaml.Text,
            RequiredItem = FindItem(yaml.RequiresItem),
            Outcome = BuildOutcome(yaml.Outcome),
            Risk = yaml.Risk == null ? new Risk() : BuildRisk(yaml.Risk)
        };

        private Risk BuildRisk(RiskYaml yaml) => new()
        {
            Chance = yaml.Chance,
            ReducedBy = yaml.ReducedBy.OrEmpty()
                .Select(entry => new ChanceReduction { Item = FindItem(entry.Key), Amount = entry.Value })
                .ToArray(),
            Outcome = BuildOutcome(yaml.Outcome)
        };

        private Outcome BuildOutcome(OutcomeYaml yaml) => new()
        {
            Text = yaml.Text,
            Effects = yaml.Effects.OrEmpty().Select(BuildEffect).ToArray()
        };

        private ChoiceEffect BuildEffect(EffectYaml yaml)
        {
            if (yaml.Stamina.HasValue)
            {
                var hazard = yaml.Hazard == null ? HazardType.None : YamlEnum.Parse<HazardType>(yaml.Hazard);
                return new ModifyStaminaEffect { Amount = yaml.Stamina.Value, Hazard = hazard };
            }

            if (yaml.Nature.HasValue)
                return new ModifyNatureScoreEffect { Amount = yaml.Nature.Value };

            if (yaml.GiveItem != null)
                return new GiveItemEffect { Item = FindItem(yaml.GiveItem) };

            return new RemoveItemEffect { Item = FindItem(yaml.RemoveItem) };
        }

        private ItemDefinition FindItem(string id) => id == null ? null : _itemsById[id];

        private static void UpdateCatalog(EventDefinition[] events)
        {
            var catalog = LoadOrCreate<EventCatalog>(DefinitionPaths.CatalogPath);
            catalog.Events = events;
            EditorUtility.SetDirty(catalog);
        }

        private static Sprite LoadIcon(string icon) =>
            string.IsNullOrEmpty(icon) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(DefinitionPaths.Icon(icon));

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void DeleteStaleAssets(string folder, IEnumerable<string> keptPaths)
        {
            var kept = new HashSet<string>(keptPaths);
            var stale = AssetDatabase.FindAssets(string.Empty, new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !kept.Contains(path))
                .ToList();

            foreach (var path in stale)
                AssetDatabase.DeleteAsset(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
