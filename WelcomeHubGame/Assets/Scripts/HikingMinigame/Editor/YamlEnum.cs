using System;
using System.Linq;

namespace HikingMinigame.Editor
{
    internal static class YamlEnum
    {
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
        {
            var name = Enum.GetNames(typeof(TEnum))
                .FirstOrDefault(candidate => string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase));

            result = name == null ? default : Enum.Parse<TEnum>(name);
            return name != null;
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct, Enum =>
            TryParse<TEnum>(value, out var result) ? result : throw new ArgumentException($"Unknown {typeof(TEnum).Name}: {value}");

        public static string Options<TEnum>() where TEnum : struct, Enum =>
            string.Join(", ", Enum.GetNames(typeof(TEnum)).Select(name => name.ToLowerInvariant()));
    }
}
