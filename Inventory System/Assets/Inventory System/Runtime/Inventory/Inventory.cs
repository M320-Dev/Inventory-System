using System;
using System.Collections;
using System.Collections.Generic;

namespace InventorySystem.Runtime
{
    public interface IInventory : IEnumerable<ISlot>
    {
        #region Indexer

        ISlot this[int index] { get; }

        #endregion

        #region Properties

        int SlotCount { get; }

        int CurrentItemCount { get; }
        int MaxItemCount { get; }

        int SlottedSlotCount { get; }
        int UnslottedSlotCount { get; }

        bool IsItemEmpty { get; }
        bool IsItemFull { get; }

        bool IsSlotFull { get; }

        #endregion

        #region Events

        event Action<IReadOnlyList<IItem>> ItemsAdded;
        event Action<IReadOnlyList<IItem>> ItemsRemoved;

        #endregion

        #region Clear

        void ClearItems();

        #endregion

        #region Add

        bool AddItem(IItem item);
        int AddItems(params IItem[] items);
        int AddItems(IEnumerable<IItem> items);

        #endregion

        #region Remove

        bool TryRemoveItem(Type itemSOType, out IItem item);
        bool TryRemoveItem<TItemSO>(out IItem item) 
            where TItemSO : IItemSO;

        IItem RemoveItem(Type itemSOType);
        IItem RemoveItem<TItemSO>() 
            where TItemSO : IItemSO;

        List<IItem> RemoveItems(Type itemSOType, int stack);
        List<IItem> RemoveItems<TItemSO>(int stack) 
            where TItemSO : IItemSO;

        #endregion

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }

    public sealed class Inventory : IInventory
    {
        #region Field

        private readonly ISlot[] _slots;

        #endregion

        #region Indexer

        public ISlot this[int index] => GetSlot(index);

        #endregion

        #region Properties

        public int SlotCount => _slots.Length;

        public int CurrentItemCount { get; private set; }
        public int MaxItemCount { get; private set; }

        public int SlottedSlotCount { get; private set; }
        public int UnslottedSlotCount => SlotCount - SlottedSlotCount;

        public bool IsItemEmpty => CurrentItemCount <= 0;
        public bool IsItemFull => CurrentItemCount >= MaxItemCount;

        public bool IsSlotFull => SlottedSlotCount >= SlotCount;

        #endregion

        #region Events

        public event Action<IReadOnlyList<IItem>> ItemsAdded;
        public event Action<IReadOnlyList<IItem>> ItemsRemoved;

        #endregion

        #region Constructor

        public Inventory(int slotCount) 
        {
            _slots = new ISlot[slotCount];

            for (int i = 0; i < slotCount; i++) ConstructSlot(i);
        }

        private void ConstructSlot(int index) 
        {
            ISlot slot = _slots[index] = new Slot();
            slot.ItemsAdded += items => SlotItemsAdded(slot, items);
            slot.ItemsRemoved += items => SlotItemsRemoved(slot, items);
        }

        private void SlotItemsAdded(ISlot slot, IReadOnlyList<IItem> items)
        {
            CurrentItemCount += items.Count;
            if (slot.CurrentSize == items.Count)
            {
                MaxItemCount += slot.ItemSO.MaxStack;
                SlottedSlotCount++;
            }
        }
        private void SlotItemsRemoved(ISlot slot, IReadOnlyList<IItem> items)
        {
            CurrentItemCount -= items.Count;
            if (slot.CurrentSize == 0)
            {
                MaxItemCount -= slot.ItemSO.MaxStack;
                SlottedSlotCount--;
            }
        }

        #endregion

        #region Get

        public ISlot GetSlot(int index)
        {
            TryThrowNotWithinSize(index);

            return _slots[index];
        }

        #endregion

        #region Clear

        public void ClearItems() 
        {
            foreach (var slot in _slots)
            {
                slot.ClearItems();
            }
        }

        #endregion

        #region Add

        public bool AddItem(IItem item) 
        {
            if (item == null || IsItemFull || ContainsItem(item)) return false;

            // Add to slotted slot
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty &&
                    slot.ItemSO == item.ItemSO &&
                    slot.AddItem(item))
                {
                    ItemsAdded?.Invoke(new IItem[] { item });
                    return true;
                }
            }
            
            // Add to unslotted slot
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty &&
                    slot.AddItem(item))
                {
                    ItemsAdded?.Invoke(new IItem[] { item });
                    return true;
                }
            }

            return false;
        }
        public int AddItems(params IItem[] items) 
        {
            return AddItems((IEnumerable<IItem>)items);
        }
        public int AddItems(IEnumerable<IItem> items) 
        {
            IEnumerator<IItem> enumerator = items.GetEnumerator();

            List<IItem> addedItems = new();

            while (enumerator.MoveNext()) 
            {
                IItem item = enumerator.Current;

                if (AddItem(item)) addedItems.Add(item);
            }

            ItemsAdded?.Invoke(addedItems);

            return addedItems.Count;
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
            if (IsItemEmpty) return null;

            foreach (var slot in _slots)
            {
                IItemSO itemSO = slot.ItemSO;
                if (itemSO.GetType() == itemSOType) 
                {
                    IItem removedItem = slot.RemoveItem();

                    ItemsRemoved?.Invoke(new IItem[] { removedItem });

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

        public List<IItem> RemoveItems(Type itemSOType, int stack) 
        {
            if (IsItemEmpty || stack < 1) return new();

            List<IItem> removedItems = new();

            foreach (var slot in _slots)
            {
                IItemSO itemSO = slot.ItemSO;
                if (itemSO.GetType() == itemSOType)
                {
                    List<IItem> _removedItems = slot.RemoveItems(stack);

                    stack -= _removedItems.Count;

                    removedItems.AddRange(_removedItems);
                }
            }

            if (removedItems.Count > 0) ItemsRemoved?.Invoke(removedItems);

            return removedItems;
        }
        public List<IItem> RemoveItems<TItemSO>(int stack)
            where TItemSO : IItemSO
        {
            return RemoveItems(typeof(TItemSO), stack);
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

        #region Within Size

        private bool WithinSize(int index)
        {
            return index >= 0 && index < SlotCount;
        }
        private void TryThrowNotWithinSize(int index)
        {
            if (!WithinSize(index)) throw new ArgumentOutOfRangeException(
                nameof(index), index, "Index must not be negative and must be less than the Size of this Inventory.");
        }

        #endregion
    }
}
