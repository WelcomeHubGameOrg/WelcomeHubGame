using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HikingMinigame.EventEditor
{
    public class EventYamlImporter
    {
        private const string YamlPath = "Assets/Data/HikingMinigame/events.yaml";
        private const string OutputFolder = "Assets/Data/HikingMinigame/Events";
        private const string IconFolder = "Assets/Graphics/HikingMinigame";
        
        private class EventYamlData
        {
            public string id;
            public string eventName;
            public string icon;
            public string description;
            public float weight;
            public int maxOccurrences;
            public List<string> requiredTags;
            public List<ChoiceYamlData> choices;
        }

        private class ChoiceYamlData
        {
            public string choiceText;
            public string consequenceDescription;
        }

        [MenuItem(("Tools/Import Events from YAML"))]
        public static void Import()
        {
            var fullPath = Path.Combine(Application.dataPath, YamlPath.Substring("Assets/".Length));
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Events YAML not found at: {fullPath}");
                return;
            }

            var yaml = File.ReadAllText(fullPath);
            
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var events = deserializer.Deserialize<List<EventYamlData>>(yaml);
            
            foreach (var data in events)
                ImportSingleEvent(data);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Imported {events.Count} event(s) from {fullPath}");
        }

        private static void ImportSingleEvent(EventYamlData data)
        {
            if (string.IsNullOrEmpty(data.id))
            {
                Debug.LogWarning($"Skipping an event with no 'id' (eventName: '{data.eventName}')");
                return;
            }
            
            var assetName = $"Event_{data.id}";
            var assetPath = $"{OutputFolder}/{assetName}.asset";
            
            var definition = AssetDatabase.LoadAssetAtPath<EventDefinition>(assetPath);
            var isNew = definition == null;
            if (isNew)
                definition = ScriptableObject.CreateInstance<EventDefinition>();
            
            if (!string.IsNullOrEmpty(data.icon)) {
                var iconPath = $"{IconFolder}/{data.icon}";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (sprite == null)
                    Debug.LogWarning($"[{assetName}] Icon path did not resolve to a sprite: {iconPath}");
                definition.Icon = sprite;
            }
            else {
                definition.Icon = null;
            }
            
            FillEventDefinitionBasics(data, definition);

            definition.Choices = new EventChoice[data.choices.Count];
            for (var i = 0; i < data.choices.Count; i++) {
                definition.Choices[i] = new EventChoice {
                    choiceText = data.choices[i].choiceText,
                    consequenceDescription = data.choices[i].consequenceDescription
                };
            }
            
            if (isNew)
                AssetDatabase.CreateAsset(definition, assetPath);
            else
                EditorUtility.SetDirty(definition);
            
        }

        private static void FillEventDefinitionBasics(EventYamlData data, EventDefinition definition)
        {
            definition.EventName = data.eventName;
            definition.Description = data.description;
            definition.Weight = data.weight;
            definition.MaxOccurrences = data.maxOccurrences;
            definition.RequiredTags = data.requiredTags?.ToArray() ?? Array.Empty<string>();
        }
    }
}
