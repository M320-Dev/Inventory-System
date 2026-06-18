using ItemSystem.Runtime;
using SlotSystem.Runtime.Core;
using System;
using System.Collections.Generic;

namespace InventorySystem.Runtime.Core
{
    internal sealed class ItemDictionary<TSlot>
        where TSlot : IReadOnlySlot
    {
        public Dictionary<TSlot, List<IItem>> dictionary { get; private set; } = new();

        public void AddSlot(TSlot slot)
        {
            dictionary.Add(slot, new());
        }

        public void AddItem(TSlot slot, IItem item)
        {
            if (dictionary.TryGetValue(slot, out List<IItem> items)) items.Add(item);
            else dictionary.Add(slot, new() { item });
        }
        public void AddItems(TSlot slot, IEnumerable<IItem> items)
        {
            foreach (var item in items) AddItem(slot, item);
        }
    }

    internal static class ItemDictionaryUtility 
    {
        public static Dictionary<ISlotTo, List<IItem>> Convert<ISlotFrom, ISlotTo>(
            Dictionary<ISlotFrom, List<IItem>> itemDictionary, 
            Converter<ISlotFrom, ISlotTo> converter
            ) 
            where ISlotFrom : IReadOnlySlot
            where ISlotTo : IReadOnlySlot
        {
            Dictionary<ISlotTo, List<IItem>> _itemDictionary = new();
            foreach (var pair in itemDictionary)
            {
                _itemDictionary.Add(converter.Invoke(pair.Key), pair.Value);
            }
            return _itemDictionary;
        }
    }
}
