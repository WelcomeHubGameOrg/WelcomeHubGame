using System.Collections.Generic;

namespace HikingMinigame.Effects
{
    public sealed class EffectContext
    {
        private readonly List<string> _messages = new();

        public EffectContext(HikingGameManager game)
        {
            Game = game;
        }

        public HikingGameManager Game { get; }
        public IReadOnlyList<string> Messages => _messages;

        public void Report(string message) => _messages.Add(message);
    }
}
