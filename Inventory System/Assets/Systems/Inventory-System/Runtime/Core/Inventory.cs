using Addons.Delegate.Runtime;
using ItemSystem.Runtime;
using SlotSystem.Runtime.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InventorySystem.Runtime.Core
{
    #region Delegates

    public delegate void ItemsUpdatedHandler<TSlot>(Dictionary<TSlot, List<IItem>> itemDictionary)
        where TSlot : IReadOnlySlot;
    public delegate TSlot SlotConstructor<TSlot>(int index)
        where TSlot : IReadOnlySlot;

    #endregion

    #region Interfaces

    public interface IReadOnlyInventory : IEnumerable
    {
        #region Indexer

        IReadOnlySlot this[int index] { get; }

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

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }
    public interface IReadOnlyInventory<out TSlot> : IReadOnlyInventory
        where TSlot : IReadOnlySlot
    {
        #region Indexer

        IReadOnlySlot IReadOnlyInventory.this[int index] => this[index];
        new TSlot this[int index] { get; }

        #endregion
    }
    public interface IInventory : IReadOnlyInventory<ISlot>
    {
        #region Events

        event ItemsUpdatedHandler<ISlot> ItemsAdded;
        event ItemsUpdatedHandler<ISlot> ItemsRemoved;

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
    }
    public interface IInventory<TSlot> : IInventory
        where TSlot : ISlot
    {
        #region Indexer

        ISlot IReadOnlyInventory<ISlot>.this[int index] => this[index];
        new TSlot this[int index] { get; }

        #endregion

        #region Events

        new event ItemsUpdatedHandler<TSlot> ItemsAdded;
        new event ItemsUpdatedHandler<TSlot> ItemsRemoved;

        #endregion

        #region Resize

        Dictionary<ISlot, List<IItem>> IInventory.Resize(int slotCount) 
        {
            return ItemDictionaryUtility.Convert(Resize(slotCount), item => (ISlot)item);
        }
        new Dictionary<TSlot, List<IItem>> Resize(int slotCount);

        #endregion

        #region Add

        Dictionary<ISlot, List<IItem>> IInventory.AddItems(params IItem[] items) 
        {
            return ItemDictionaryUtility.Convert(AddItems(items), item => (ISlot)item);
        }
        Dictionary<ISlot, List<IItem>> IInventory.AddItems(IEnumerable<IItem> items) 
        {
            return ItemDictionaryUtility.Convert(AddItems(items), item => (ISlot)item);
        }

        new Dictionary<TSlot, List<IItem>> AddItems(params IItem[] items);
        new Dictionary<TSlot, List<IItem>> AddItems(IEnumerable<IItem> items);

        #endregion

        #region Remove

        Dictionary<ISlot, List<IItem>> IInventory.RemoveItems(Type itemSOType, int stack) 
        {
            return ItemDictionaryUtility.Convert(RemoveItems(itemSOType, stack), item => (ISlot)item);
        }
        Dictionary<ISlot, List<IItem>> IInventory.RemoveItems<TItemSO>(int stack)
        {
            return ItemDictionaryUtility.Convert(RemoveItems<TItemSO>(stack), item => (ISlot)item);
        }

        Dictionary<ISlot, List<IItem>> IInventory.RemoveAllItems() 
        {
            return ItemDictionaryUtility.Convert(RemoveAllItems(), item => (ISlot)item);
        }

        new Dictionary<TSlot, List<IItem>> RemoveItems(Type itemSOType, int stack);
        new Dictionary<TSlot, List<IItem>> RemoveItems<TItemSO>(int stack)
            where TItemSO : IItemSO;

        new Dictionary<TSlot, List<IItem>> RemoveAllItems();

        #endregion
    }

    #endregion

    public class Inventory<TSlot> : IInventory<TSlot>
        where TSlot : ISlot
    {
        #region Fields

        private SlotConstructor<TSlot> _slotConstructor;
        private TSlot[] _slots;

        #endregion

        #region Indexer

        public TSlot this[int index]
        {
            get
            {
                TryThrowNotWithinSizeException(index);
                return _slots[index];
            }
        }

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

        private DelegateAdapter<ItemsUpdatedHandler<ISlot>, ItemsUpdatedHandler<TSlot>> itemsAddedAdapter;
        private DelegateAdapter<ItemsUpdatedHandler<ISlot>, ItemsUpdatedHandler<TSlot>> itemsRemovedAdapter;

        event ItemsUpdatedHandler<ISlot> IInventory.ItemsAdded 
        {
            add => itemsAddedAdapter.Add(value);
            remove => itemsAddedAdapter.Remove(value);
        }
        event ItemsUpdatedHandler<ISlot> IInventory.ItemsRemoved 
        {
            add => itemsRemovedAdapter.Add(value);
            remove => itemsRemovedAdapter.Remove(value);
        }

        public event ItemsUpdatedHandler<TSlot> ItemsAdded;
        public event ItemsUpdatedHandler<TSlot> ItemsRemoved;

        #endregion

        #region Constructor

        public Inventory(int slotCount, SlotConstructor<TSlot> slotConstructor)
        {
            TryThrowNonPositiveSlotCountException(slotCount);

            _slotConstructor = slotConstructor ?? throw new ArgumentNullException(nameof(slotConstructor));

            _slots = new TSlot[slotCount];

            for (int i = 0; i < slotCount; i++) ConstructSlot(i);

            ConstructDelegateAdapters();
        }

        private TSlot ConstructSlot(int index)
        {
            TSlot slot = _slots[index] = _slotConstructor.Invoke(index);

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

        private void ConstructDelegateAdapters() 
        {
            itemsAddedAdapter = new(
                handler => itemDictionary => handler.Invoke(ItemDictionaryUtility.Convert(itemDictionary, item => (ISlot)item)),
                handler => ItemsAdded += handler,
                handler => ItemsAdded -= handler);

            itemsRemovedAdapter = new(
                handler => itemDictionary => handler.Invoke(ItemDictionaryUtility.Convert(itemDictionary, item => (ISlot)item)),
                handler => ItemsRemoved += handler,
                handler => ItemsRemoved -= handler);
        }

        #endregion

        #region Resize

        public Dictionary<TSlot, List<IItem>> Resize(int slotCount)
        {
            TryThrowNonPositiveSlotCountException(slotCount);

            int slotCountDiff = slotCount - SlotCount;

            ItemDictionary<TSlot> addedOrRemoveditems = new();

            if (slotCountDiff > 0)
            {
                Array.Resize(ref _slots, slotCount);

                for (int i = SlotCount; i < slotCount; i++)
                {
                    TSlot slot = ConstructSlot(i);

                    addedOrRemoveditems.AddSlot(slot);
                }
            }
            else if (slotCountDiff < 0)
            {
                for (int i = SlotCount - 1; i >= slotCount; i--)
                {
                    TSlot slot = _slots[i];

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
                ItemDictionary<TSlot> itemDictionary = new();
                itemDictionary.AddItem(slot, item);
                ItemsAdded?.Invoke(itemDictionary.dictionary);
            });
        }
        public Dictionary<TSlot, List<IItem>> AddItems(params IItem[] items)
        {
            return AddItems((IEnumerable<IItem>)items);
        }
        public Dictionary<TSlot, List<IItem>> AddItems(IEnumerable<IItem> items)
        {
            IEnumerator<IItem> enumerator = items.GetEnumerator();

            ItemDictionary<TSlot> addedItems = new();

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

        private bool AddItemHelper(IItem item, Action<TSlot, IItem> itemAdded)
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

                    ItemDictionary<TSlot> itemDictionary = new();
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

        public Dictionary<TSlot, List<IItem>> RemoveItems(Type itemSOType, int stack)
        {
            if (itemSOType == null || IsEmpty || stack < 1) return new();

            ItemDictionary<TSlot> removedItems = new();

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
        public Dictionary<TSlot, List<IItem>> RemoveItems<TItemSO>(int stack)
            where TItemSO : IItemSO
        {
            return RemoveItems(typeof(TItemSO), stack);
        }

        public Dictionary<TSlot, List<IItem>> RemoveAllItems()
        {
            if (IsEmpty) return new();

            ItemDictionary<TSlot> removedItems = new();

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

        public IEnumerator<TSlot> GetEnumerator()
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

    public static class InventoryUtility
    {
        #region Swap

        public static void Swap(this IReadOnlyInventory<ISwappableSlot> inventory, int indexA, int indexB)
        {
            if (inventory == null || 
                indexA == indexB) return;

            SwapInternal(inventory, indexA, indexB);
        }
        internal static void SwapInternal(this IReadOnlyInventory<ISwappableSlot> inventory, int indexA, int indexB)
        {
            SlotUtility.SwapInternal(inventory[indexA], inventory[indexB]);
        }

        #endregion

        #region Transfer

        public static List<IItem> Transfer(this IReadOnlyInventory<ISlot> inventory, int indexFrom, int indexTo)
        {
            if (inventory == null ||
                indexFrom == indexTo) return new();

            return TransferInternal(inventory, indexFrom, indexTo);
        }
        public static List<IItem> TransferInternal(this IReadOnlyInventory<ISlot> inventory, int indexFrom, int indexTo)
        {
            return SlotUtility.TransferInternal(inventory[indexFrom], inventory[indexTo]);
        }

        #endregion
    }

    public static class InventoryFactory 
    {
        public static Inventory<TSlot> ParameterlessSlotConstructor<TSlot>(int slotCount)
            where TSlot : ISlot, new()
        {
            return new(slotCount, index => new());
        }
    }
}
