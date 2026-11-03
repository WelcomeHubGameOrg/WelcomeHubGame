using HikingMinigame.Items;

namespace HikingMinigame
{
    public readonly struct StaminaChange
    {
        public StaminaChange(int delta, ItemDefinition protectedBy, ItemDefinition revivedBy)
        {
            Delta = delta;
            ProtectedBy = protectedBy;
            RevivedBy = revivedBy;
        }

        public int Delta { get; }
        public ItemDefinition ProtectedBy { get; }
        public ItemDefinition RevivedBy { get; }
    }
}
