using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace HikingMinigame.Editor
{
    internal sealed class DefinitionFileYaml
    {
        public int Version { get; set; }
        public List<ItemYaml> Items { get; set; }
        public List<EventYaml> Events { get; set; }
    }

    internal sealed class ItemYaml
    {
        [YamlIgnore] public string SourceFile { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public List<string> ProtectsAgainst { get; set; }
        public int ReviveStamina { get; set; }
        public bool SingleUse { get; set; }
    }

    internal sealed class EventYaml
    {
        [YamlIgnore] public string SourceFile { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public float Weight { get; set; } = 1f;
        public int? MaxOccurrences { get; set; }
        public string Trigger { get; set; } = "random";
        public List<ChoiceYaml> Choices { get; set; }
    }

    internal sealed class ChoiceYaml
    {
        public string Text { get; set; }
        public string RequiresItem { get; set; }
        public OutcomeYaml Outcome { get; set; }
        public RiskYaml Risk { get; set; }
    }

    internal sealed class OutcomeYaml
    {
        public string Text { get; set; }
        public List<EffectYaml> Effects { get; set; }
    }

    internal sealed class RiskYaml
    {
        public float Chance { get; set; }
        public Dictionary<string, float> ReducedBy { get; set; }
        public OutcomeYaml Outcome { get; set; }
    }

    internal sealed class EffectYaml
    {
        public int? Stamina { get; set; }
        public string Hazard { get; set; }
        public int? Nature { get; set; }
        public string GiveItem { get; set; }
        public string RemoveItem { get; set; }

        [YamlIgnore]
        public int ActionCount => new object[] { Stamina, Nature, GiveItem, RemoveItem }.Count(action => action != null);
    }
}
