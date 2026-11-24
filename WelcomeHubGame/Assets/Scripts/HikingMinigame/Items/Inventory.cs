using System;
using System.Collections.Generic;
using System.Linq;

namespace HikingMinigame.Items
{
    public sealed class Inventory
    {
        private readonly List<ItemDefinition> _items = new();

        public Inventory(int capacity)
        {
            Capacity = capacity;
        }

        public event Action Changed;

        public int Capacity { get; }
        public IReadOnlyList<ItemDefinition> Items => _items;
        public bool IsFull => _items.Count >= Capacity;

        public bool Has(ItemDefinition item) => _items.Contains(item);

        public bool TryAdd(ItemDefinition item)
        {
            if (IsFull)
                return false;

            _items.Add(item);
            Changed?.Invoke();
            return true;
        }

        public bool Remove(ItemDefinition item)
        {
            if (!_items.Remove(item))
                return false;

            Changed?.Invoke();
            return true;
        }

        public ItemDefinition FindProtectionAgainst(HazardType hazard) =>
            _items.FirstOrDefault(item => item.Protects(hazard));

        public ItemDefinition FindRevive() => _items.FirstOrDefault(item => item.CanRevive);

        public void Use(ItemDefinition item)
        {
            if (item.SingleUse)
                Remove(item);
        }
    }
}
