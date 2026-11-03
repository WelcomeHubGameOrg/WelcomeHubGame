using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HikingMinigame.Editor
{
    public static class DefinitionImporter
    {
        private const int SupportedVersion = 1;

        [MenuItem("Tools/Hiking/Import Definitions")]
        public static void Import()
        {
            var errors = new List<string>();
            var files = LoadFiles(errors);
            var items = files.SelectMany(file => file.Items.OrEmpty()).ToList();
            var events = files.SelectMany(file => file.Events.OrEmpty()).ToList();

            errors.AddRange(DefinitionValidator.Validate(items, events));

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Debug.LogError(error);

                Debug.LogError($"Import aborted with {errors.Count} error(s). No assets were changed.");
                return;
            }

            new DefinitionAssetWriter().Write(items, events);
            AssetDatabase.SaveAssets();

            Debug.Log($"Imported {items.Count} item(s) and {events.Count} event(s) from {files.Count} file(s).");
        }

        private static List<DefinitionFileYaml> LoadFiles(List<string> errors)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var files = new List<DefinitionFileYaml>();
            foreach (var path in Directory.GetFiles(DefinitionPaths.DefinitionFolder, "*.yaml"))
            {
                var fileName = Path.GetFileName(path);
                try
                {
                    var file = deserializer.Deserialize<DefinitionFileYaml>(File.ReadAllText(path));
                    if (file == null || file.Version != SupportedVersion)
                    {
                        errors.Add($"{fileName}: must start with 'version: {SupportedVersion}'.");
                        continue;
                    }

                    TagSourceFile(file, fileName);
                    files.Add(file);
                }
                catch (YamlException exception)
                {
                    var reason = (exception.InnerException ?? exception).Message;
                    errors.Add($"{fileName}, line {exception.Start.Line}: {reason}");
                }
            }

            return files;
        }

        private static void TagSourceFile(DefinitionFileYaml file, string fileName)
        {
            foreach (var item in file.Items.OrEmpty())
                item.SourceFile = fileName;

            foreach (var definition in file.Events.OrEmpty())
                definition.SourceFile = fileName;
        }
    }
}
