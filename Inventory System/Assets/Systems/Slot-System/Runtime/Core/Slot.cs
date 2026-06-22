using ItemSystem.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SlotSystem.Runtime.Core
{
    #region Delegates

    public delegate void ItemsRemoveHandler(IItemSO previousItemSO, IReadOnlyList<IItem> removedItems);
    public delegate void UnslotHandler(IItemSO previousItemSO, IReadOnlyList<IItem> removedItems);

    #endregion

    #region Interfaces

    public interface IReadOnlySlot : IEnumerable<IItem>
    {
        #region Indexer

        IItem this[int index] { get; }

        #endregion

        #region Field Property

        IItemSO ItemSO { get; }

        #endregion

        #region Properties

        int Stack { get; }
        int MaxStack { get; }

        bool IsEmpty { get; }
        bool IsFull { get; }

        #endregion

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }
    public interface ISlotInternal : IReadOnlySlot
    {
        #region Field Properties

        internal ICollection<IItem> Items { get; set; }

        IItemSO IReadOnlySlot.ItemSO => ItemSO;
        internal new IItemSO ItemSO { get; set; }

        #endregion

        #region Properties

        int IReadOnlySlot.Stack => Stack;
        internal new int Stack { get; set; }

        #endregion

        #region Swap With

        void SwapWith(ISlotInternal other);

        #endregion

        #region Transfer To

        List<IItem> TransferTo(ISlot other);

        #endregion
    }
    public interface ISlot : ISlotInternal
    {
        #region Events

        event Action<IReadOnlyList<IItem>> ItemsAdded;
        event ItemsRemoveHandler ItemsRemoved;

        event Action<IReadOnlyList<IItem>> Slotted;
        event UnslotHandler Unslotted;

        #endregion

        #region Add

        bool AddItem(IItem item);
        int AddItems(params IItem[] items);
        int AddItems(IEnumerable<IItem> items);

        #endregion

        #region Remove

        bool TryRemoveItem(out IItem item);
        IItem RemoveItem();
        List<IItem> RemoveItems(int stack);
        IItem[] RemoveAllItems();

        #endregion
    }

    #endregion

    public sealed class Slot : ISlot
    {
        #region Field

        private IItem[] _items = Array.Empty<IItem>();

        #endregion

        #region Indexer

        public IItem this[int index]
        {
            get
            {
                TryThrowNotWithinStack(index);
                return _items[index];
            }
        }

        #endregion

        #region Field Properties

        ICollection<IItem> ISlotInternal.Items
        {
            get => _items; 
            set 
            {
                ICollection<IItem> collection = value;
                _items = new IItem[collection.Count];
                collection.CopyTo(_items, 0);
            } 
        }

        IItemSO ISlotInternal.ItemSO { get => ItemSO; set => ItemSO = value; }

        public IItemSO ItemSO { get; private set; }

        #endregion

        #region Properties

        int ISlotInternal.Stack { get => Stack; set => Stack = value; }

        public int Stack { get; private set; }
        public int MaxStack => _items.Length;

        public bool IsEmpty => ItemSO == null || Stack <= 0;
        public bool IsFull => ItemSO != null && Stack >= MaxStack;

        #endregion

        #region Events

        public event Action<IReadOnlyList<IItem>> ItemsAdded;
        public event ItemsRemoveHandler ItemsRemoved;

        public event Action<IReadOnlyList<IItem>> Slotted;
        public event UnslotHandler Unslotted;

        #endregion

        #region Constructor

        public Slot() 
        {
            ItemsAdded += items => 
            {
                if (Stack == items.Count)
                {
                    Slotted?.Invoke(items);
                }
            };
            ItemsRemoved += (previousItemSO, removedItems) =>
            {
                if (Stack == 0)
                {
                    Unslotted?.Invoke(previousItemSO, removedItems);
                }
            };
        }

        #endregion

        #region Add

        public bool AddItem(IItem item)
        {
            if (item == null || IsFull || ContainsItem(item)) return false;

            if (IsEmpty)
            {
                SetItemSOAndResize(item.ItemSO);

                _items[Stack++] = item;

                ItemsAdded?.Invoke(new IItem[] { item });

                return true;
            }
            else if (!IsFull && ItemSO == item.ItemSO)
            {
                _items[Stack++] = item;

                ItemsAdded?.Invoke(new IItem[] { item });

                return true;
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

            if (IsEmpty && enumerator.MoveNext())
            {
                IItem item = enumerator.Current;

                if (item != null && 
                    item.ItemSO != null)
                {
                    SetItemSOAndResize(item.ItemSO);

                    _items[Stack++] = item;

                    addedItems.Add(item);
                }
            }

            while (!IsFull && enumerator.MoveNext())
            {
                IItem item = enumerator.Current;

                if (item != null && 
                    !ContainsItem(item) && 
                    ItemSO == item.ItemSO)
                {
                    _items[Stack++] = item;

                    addedItems.Add(item);
                }
            }

            ItemsAdded?.Invoke(addedItems);

            return addedItems.Count;
        }

        #endregion

        #region Remove

        public bool TryRemoveItem(out IItem item)
        {
            return (item = RemoveItem()) != null;
        }
        public IItem RemoveItem()
        {
            if (IsEmpty) return null;

            IItem removedItem = _items[--Stack];

            IItemSO previousItemSO = ItemSO;
            if (IsEmpty) SetItemSOAndResize(null);
            else _items[Stack] = null;

            ItemsRemoved?.Invoke(previousItemSO, new IItem[] { removedItem });

            return removedItem;
        }
        public List<IItem> RemoveItems(int stack)
        {
            if (IsEmpty || stack < 1) return new();

            stack = Math.Min(stack, Stack);
            int startIndex = Stack - stack;

            List<IItem> removedItems = new();

            IItemSO previousItemSO = ItemSO;

            for (int i = Stack - 1; i >= startIndex; i--)
            {
                IItem removedItem = _items[--Stack];

                removedItems.Add(removedItem);

                if (IsEmpty) SetItemSOAndResize(null);
                else _items[Stack] = null;
            }

            ItemsRemoved?.Invoke(previousItemSO, removedItems);

            return removedItems;
        }
        public IItem[] RemoveAllItems() 
        {
            if (IsEmpty) return Array.Empty<IItem>();

            IItem[] removedItems = new IItem[Stack];


            for (int i = 0; i < Stack; i++)
            {
                removedItems[i] = _items[i];
            }

            Stack = 0;

            IItemSO previousItemSO = ItemSO;
            SetItemSOAndResize(null);

            ItemsRemoved?.Invoke(previousItemSO, removedItems);

            return removedItems;
        }

        #endregion

        #region Contains

        public bool ContainsItem(IItem item)
        {
            for (int i = 0; i < Stack; i++)
            {
                if (_items[i] == item) return true;
            }
            return false;
        }

        #endregion

        #region Swap With

        public void SwapWith(ISlotInternal other)
        {
            SlotUtility.SwapMatchingType(this, other);
        }

        #endregion

        #region Transfer To

        public List<IItem> TransferTo(ISlot other)
        {
            return SlotUtility.Transfer(this, other);
        }

        #endregion

        #region Enumerator

        public IEnumerator<IItem> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Helpers

        private void SetItemSOAndResize(IItemSO itemSO)
        {
            if (ItemSO != itemSO)
            {
                if (itemSO != null) Array.Resize(ref _items, itemSO.MaxStack);
                else Array.Resize(ref _items, 0);
            }
            ItemSO = itemSO;
        }

        private void TryThrowNotWithinStack(int index)
        {
            if (!WithinStack(index)) throw new ArgumentOutOfRangeException(
                nameof(index), index, "Index must not be negative and must be less than the Stack of this ItemSlot.");
        }
        private bool WithinStack(int index) 
        {
            return index >= 0 && index < Stack;
        }

        #endregion
    }

    public static class SlotUtility
    {
        public readonly struct TransferOrSwapResult 
        {
            public enum Type 
            {
                Transfer,
                Swap
            }

            public readonly List<IItem> items;
            public readonly Type type;

            public TransferOrSwapResult(List<IItem> items, Type type) 
            {
                this.items = items;
                this.type = type;
            }

            public static TransferOrSwapResult Transfer(List<IItem> items)
            {
                return new(items, Type.Transfer);
            }
            public static TransferOrSwapResult Swap = new(new(), Type.Swap);
        }

        #region Transfer Or Swap

        public static TransferOrSwapResult TransferOrSwapMatchingType(ISlot from, ISlot to)
        {
            if (from == null ||
                to == null ||
                from == to ||
                from.GetType() != to.GetType()) return new();

            return TransferOrSwapInternal(from, to);
        }
        public static TransferOrSwapResult TransferOrSwap(ISlot from, ISlot to) 
        {
            if (from == null ||
                to == null ||
                from == to) return new();

            return TransferOrSwapInternal(from, to);
        }
        public static TransferOrSwapResult TransferOrSwapInternal(ISlot from, ISlot to)
        {
            if (from.ItemSO == to.ItemSO)
            {
                List<IItem> transferredItems = TransferInternal(from, to);
                return TransferOrSwapResult.Transfer(transferredItems);
            }
            else
            {
                SwapInternal(from, to);
                return TransferOrSwapResult.Swap;
            }
        }

        #endregion

        #region Swap

        public static void SwapMatchingType(ISlotInternal a, ISlotInternal b)
        {
            if (a == null ||
                b == null ||
                a == b ||
                a.GetType() != b.GetType()) return;

            SwapInternal(a, b);
        }
        public static void Swap(ISlotInternal a, ISlotInternal b)
        {
            if (a == null ||
                b == null ||
                a == b) return;

            SwapInternal(a, b);
        }
        public static void SwapInternal(ISlotInternal a, ISlotInternal b)
        {
            (a.Items, b.Items) = (b.Items, a.Items);
            (a.ItemSO, b.ItemSO) = (b.ItemSO, a.ItemSO);
            (a.Stack, b.Stack) = (b.Stack, a.Stack);
        }

        #endregion

        #region Transfer

        public static List<IItem> TransferMatchingType(ISlot from, ISlot to)
        {
            if (from == null ||
                to == null ||
                from == to ||
                from.GetType() != to.GetType()) return new();

            return TransferInternal(from, to);
        }
        public static List<IItem> Transfer(ISlot from, ISlot to)
        {
            if (from == null ||
                to == null ||
                from == to) return new();

            return TransferInternal(from, to);
        }
        public static List<IItem> TransferInternal(ISlot from, ISlot to)
        {
            int stack = !to.IsEmpty ? to.MaxStack - to.Stack : from.Stack;
            List<IItem> items = from.RemoveItems(stack);

            to.AddItems(items);
            return items;
        }

        #endregion
    }
}
