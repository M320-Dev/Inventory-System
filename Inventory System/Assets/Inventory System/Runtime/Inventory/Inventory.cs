using System;
using System.Collections;
using System.Collections.Generic;

namespace InventorySystem.Runtime
{
    internal sealed class ItemDictionary 
    {
        public Dictionary<ISlot, List<IItem>> dictionary { get; private set; } = new();

        public void AddSlot(ISlot slot)
        {
            dictionary.Add(slot, new());
        }

        public void AddItem(ISlot slot, IItem item) 
        {
            if (dictionary.TryGetValue(slot, out List<IItem> items)) items.Add(item);
            else dictionary.Add(slot, new() { item });
        }
        public void AddItems(ISlot slot, IEnumerable<IItem> items)
        {
            foreach (var item in items) AddItem(slot, item);
        }
    }

    public delegate void ItemsUpdatedHandler(Dictionary<ISlot, List<IItem>> itemDictionary);

    public interface IInventory : IEnumerable<ISlot>
    {
        #region Indexer

        ISlot this[int index] { get; }

        #endregion

        #region Properties

        int SlotCount { get; }

        int ItemCount { get; }
        int MaxItemCount { get; }

        int SlottedSlotCount { get; }
        int UnslottedSlotCount { get; }

        bool IsItemFull { get; }
        bool IsSlotFull { get; }

        bool IsEmpty { get; }
        bool IsFull { get; }

        #endregion

        #region Events

        event ItemsUpdatedHandler ItemsAdded;
        event ItemsUpdatedHandler ItemsRemoved;

        #endregion

        #region Resize

        Dictionary<ISlot, List<IItem>> Resize(int slotCount);

        #endregion

        #region Add

        bool AddItem(IItem item);
        Dictionary<ISlot, List<IItem>> AddItems(params IItem[] items);
        Dictionary<ISlot, List<IItem>> AddItems(IEnumerable<IItem> items);

        #endregion

        #region Remove

        bool TryRemoveItem(Type itemSOType, out IItem item);
        bool TryRemoveItem<TItemSO>(out IItem item) 
            where TItemSO : IItemSO;

        IItem RemoveItem(Type itemSOType);
        IItem RemoveItem<TItemSO>() 
            where TItemSO : IItemSO;

        Dictionary<ISlot, List<IItem>> RemoveItems(Type itemSOType, int stack);
        Dictionary<ISlot, List<IItem>> RemoveItems<TItemSO>(int stack) 
            where TItemSO : IItemSO;

        Dictionary<ISlot, List<IItem>> RemoveAllItems();

        #endregion

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }

    public sealed class Inventory : IInventory
    {
        #region Field

        private ISlot[] _slots;

        #endregion

        #region Indexer

        public ISlot this[int index] => GetSlot(index);

        #endregion

        #region Properties

        public int SlotCount => _slots.Length;

        public int ItemCount { get; private set; }
        public int MaxItemCount { get; private set; }

        public int SlottedSlotCount { get; private set; }
        public int UnslottedSlotCount => SlotCount - SlottedSlotCount;

        public bool IsItemFull => MaxItemCount != -1 && ItemCount >= MaxItemCount;
        public bool IsSlotFull => SlottedSlotCount >= SlotCount;

        public bool IsEmpty => ItemCount <= 0;
        public bool IsFull => IsItemFull && IsSlotFull;

        #endregion

        #region Events

        public event ItemsUpdatedHandler ItemsAdded;
        public event ItemsUpdatedHandler ItemsRemoved;

        #endregion

        #region Constructor

        public Inventory(int slotCount) 
        {
            TryThrowNonPositiveSlotCountException(slotCount);

            _slots = new ISlot[slotCount];

            for (int i = 0; i < slotCount; i++) ConstructSlot(i);
        }

        private ISlot ConstructSlot(int index) 
        {
            ISlot slot = _slots[index] = new Slot();

            slot.ItemsAdded += items => ItemCount += items.Count;
            slot.ItemsRemoved += (previousItemSO, removedItems) => ItemCount -= removedItems.Count;

            slot.Slotted += items => 
            {
                Slot(slot.ItemSO.MaxStack);
            };
            slot.Unslotted += (previousItemSO, removedItems) => 
            {
                Unslot(previousItemSO.MaxStack);
            };

            return slot;
        }

        #endregion

        #region Get

        public ISlot GetSlot(int index)
        {
            TryThrowNotWithinSizeException(index);

            return _slots[index];
        }

        #endregion

        #region Resize

        public Dictionary<ISlot, List<IItem>> Resize(int slotCount) 
        {
            TryThrowNonPositiveSlotCountException(slotCount);

            int slotCountDiff = slotCount - SlotCount;

            ItemDictionary addedOrRemoveditems = new();

            if (slotCountDiff > 0)
            {
                Array.Resize(ref _slots, slotCount);

                for (int i = SlotCount; i < slotCount; i++)
                {
                    ISlot slot = ConstructSlot(i);

                    addedOrRemoveditems.AddSlot(slot);
                }
            }
            else if (slotCountDiff < 0) 
            {
                for (int i = SlotCount - 1; i >= slotCount; i--)
                {
                    ISlot slot = _slots[i];

                    addedOrRemoveditems.AddItems(slot, slot.RemoveAllItems());
                }

                Array.Resize(ref _slots, slotCount);

                ItemsRemoved?.Invoke(addedOrRemoveditems.dictionary);

                return addedOrRemoveditems.dictionary;
            }

            return addedOrRemoveditems.dictionary;
        }

        #endregion

        #region Add

        public bool AddItem(IItem item) 
        {
            return AddItemHelper(item, (slot, item) =>
            {
                ItemDictionary itemDictionary = new();
                itemDictionary.AddItem(slot, item);
                ItemsAdded?.Invoke(itemDictionary.dictionary);
            });
        }
        public Dictionary<ISlot, List<IItem>> AddItems(params IItem[] items) 
        {
            return AddItems((IEnumerable<IItem>)items);
        }
        public Dictionary<ISlot, List<IItem>> AddItems(IEnumerable<IItem> items) 
        {
            IEnumerator<IItem> enumerator = items.GetEnumerator();

            ItemDictionary addedItems = new();

            while (enumerator.MoveNext()) 
            {
                IItem item = enumerator.Current;

                AddItemHelper(item, (slot, item) => 
                {
                    addedItems.AddItem(slot, item);
                });
            }

            ItemsAdded?.Invoke(addedItems.dictionary);

            return addedItems.dictionary;
        }

        private bool AddItemHelper(IItem item, Action<ISlot, IItem> itemAdded)
        {
            if (item == null || IsFull || ContainsItem(item)) return false;

            // Add to slotted slot
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty &&
                    slot.ItemSO == item.ItemSO &&
                    slot.AddItem(item))
                {
                    itemAdded.Invoke(slot, item);
                    return true;
                }
            }

            // Add to unslotted slot
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty &&
                    slot.AddItem(item))
                {
                    itemAdded.Invoke(slot, item);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Remove

        public bool TryRemoveItem(Type itemSOType, out IItem item) 
        {
            return (item = RemoveItem(itemSOType)) != null;
        }
        public bool TryRemoveItem<TItemSO>(out IItem item)
            where TItemSO : IItemSO
        {
            return TryRemoveItem(typeof(TItemSO), out item);
        }

        public IItem RemoveItem(Type itemSOType) 
        {
            if (itemSOType == null) return null;

            if (IsEmpty) return null;

            foreach (var slot in _slots)
            {
                IItemSO itemSO = slot.ItemSO;
                if (itemSO?.GetType() == itemSOType) 
                {
                    IItem removedItem = slot.RemoveItem();

                    ItemDictionary itemDictionary = new();
                    itemDictionary.AddItem(slot, removedItem);
                    ItemsRemoved?.Invoke(itemDictionary.dictionary);

                    return removedItem;
                }
            }

            return null;
        }
        public IItem RemoveItem<TItemSO>()
            where TItemSO : IItemSO
        {
            return RemoveItem(typeof(TItemSO));
        }

        public Dictionary<ISlot, List<IItem>> RemoveItems(Type itemSOType, int stack) 
        {
            if (itemSOType == null || IsEmpty || stack < 1) return new();

            ItemDictionary removedItems = new();

            foreach (var slot in _slots)
            {
                IItemSO itemSO = slot.ItemSO;
                if (itemSO.GetType() == itemSOType)
                {
                    List<IItem> _removedItems = slot.RemoveItems(stack);
                    stack -= _removedItems.Count;
                    removedItems.AddItems(slot, _removedItems);
                }
            }

            if (removedItems.dictionary.Count > 0) ItemsRemoved?.Invoke(removedItems.dictionary);

            return removedItems.dictionary;
        }
        public Dictionary<ISlot, List<IItem>> RemoveItems<TItemSO>(int stack)
            where TItemSO : IItemSO
        {
            return RemoveItems(typeof(TItemSO), stack);
        }

        public Dictionary<ISlot, List<IItem>> RemoveAllItems() 
        {
            if (IsEmpty) return new();

            ItemDictionary removedItems = new();

            foreach (var slot in _slots)
            {
                removedItems.AddItems(slot, slot.RemoveAllItems());
            }

            ItemsRemoved?.Invoke(removedItems.dictionary);

            return removedItems.dictionary;
        }

        #endregion

        #region Contains

        public bool ContainsItem(IItem item) 
        {
            foreach (var slot in _slots)
            {
                if (slot.ContainsItem(item)) return true;
            }
            return false;
        }

        #endregion

        #region Enumerator

        public IEnumerator<ISlot> GetEnumerator()
        {
            foreach (var slot in _slots)
            {
                yield return slot;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Helpers

        private void Slot(int MaxStack) 
        {
            MaxItemCount += MaxStack;
            SlottedSlotCount++;
        }
        private void Unslot(int MaxStack)
        {
            MaxItemCount -= MaxStack;
            SlottedSlotCount--;
        }

        private void TryThrowNonPositiveSlotCountException(int slotCount)
        {
            if (slotCount < 1) throw new ArgumentOutOfRangeException(
                nameof(slotCount), slotCount, "Slot Count must be positive.");
        }

        private void TryThrowNotWithinSizeException(int index)
        {
            if (!WithinSize(index)) throw new ArgumentOutOfRangeException(
                nameof(index), index, "Index must not be negative and must be less than the Size of this Inventory.");
        }
        private bool WithinSize(int index)
        {
            return index >= 0 && index < SlotCount;
        }

        #endregion
    }
}
