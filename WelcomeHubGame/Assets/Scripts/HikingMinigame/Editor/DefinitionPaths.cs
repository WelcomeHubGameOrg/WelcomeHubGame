namespace HikingMinigame.Editor
{
    internal static class DefinitionPaths
    {
        public const string DefinitionFolder = "Assets/Data/HikingMinigame/Definitions";
        public const string IconFolder = "Assets/Graphics/HikingMinigame";
        public const string ItemFolder = "Assets/Data/HikingMinigame/Generated/Items";
        public const string EventFolder = "Assets/Data/HikingMinigame/Generated/Events";
        public const string CatalogPath = "Assets/Data/HikingMinigame/EventCatalog.asset";

        public static string Icon(string relativePath) => $"{IconFolder}/{relativePath}";
        public static string Item(string id) => $"{ItemFolder}/Item_{id}.asset";
        public static string Event(string id) => $"{EventFolder}/Event_{id}.asset";
    }
}
