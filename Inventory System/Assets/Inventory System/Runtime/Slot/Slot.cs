using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Runtime
{
    public interface ISlot : IEnumerable<IItem>
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

        #region Events

        event Action<IReadOnlyList<IItem>> ItemsAdded;
        event Action<IReadOnlyList<IItem>> ItemsRemoved;

        event Action<IReadOnlyList<IItem>> Slotted;
        event Action<IReadOnlyList<IItem>> Unslotted;

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

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }

    public sealed class Slot : ISlot
    {
        #region Field

        private IItem[] _items = Array.Empty<IItem>();

        #endregion

        #region Field Property

        public IItemSO ItemSO { get; private set; }

        #endregion

        #region Indexer

        public IItem this[int index] => GetItem(index);

        #endregion

        #region Properties

        public int Stack { get; private set; }
        public int MaxStack => _items.Length;

        public bool IsEmpty => Stack <= 0;
        public bool IsFull => Stack >= MaxStack;

        #endregion

        #region Events

        public event Action<IReadOnlyList<IItem>> ItemsAdded;
        public event Action<IReadOnlyList<IItem>> ItemsRemoved;

        public event Action<IReadOnlyList<IItem>> Slotted;
        public event Action<IReadOnlyList<IItem>> Unslotted;

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
            ItemsRemoved += items =>
            {
                if (Stack == 0)
                {
                    Unslotted?.Invoke(items);
                }
            };
        }

        #endregion

        #region Get

        public IItem GetItem(int index)
        {
            TryThrowNotWithinStack(index);

            return _items[index];
        }

        #endregion

        #region Add

        public bool AddItem(IItem item)
        {
            if (item == null || IsFull || ContainsItem(item)) return false;

            if (IsEmpty)
            {
                ItemSO = item.ItemSO;
                Array.Resize(ref _items, ItemSO.MaxStack);

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

                if (item != null)
                {
                    ItemSO = item.ItemSO;
                    Array.Resize(ref _items, ItemSO.MaxStack);

                    _items[Stack++] = item;

                    addedItems.Add(item);
                }
            }

            while (!IsFull && enumerator.MoveNext())
            {
                IItem item = enumerator.Current;

                if (item != null && !ContainsItem(item) && ItemSO == item.ItemSO)
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

            if (IsEmpty) Array.Resize(ref _items, 0);
            else _items[Stack] = null;

            ItemsRemoved?.Invoke(new IItem[] { removedItem });

            return removedItem;
        }
        public List<IItem> RemoveItems(int stack)
        {
            if (IsEmpty || stack < 1) return new();

            stack = Math.Min(stack, Stack);
            int startIndex = Stack - stack;

            List<IItem> removedItems = new();

            for (int i = Stack - 1; i >= startIndex; i--)
            {
                IItem removedItem = _items[--Stack];

                removedItems.Add(removedItem);

                if (IsEmpty) Array.Resize(ref _items, 0);
                else _items[Stack] = null;
            }

            ItemsRemoved?.Invoke(removedItems);

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

            Array.Resize(ref _items, 0);

            ItemsRemoved?.Invoke(removedItems);

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

        #region Within Current Size

        private bool WithinStack(int index) 
        {
            return index >= 0 && index < Stack;
        }
        private void TryThrowNotWithinStack(int index)
        {
            if (!WithinStack(index)) throw new ArgumentOutOfRangeException(
                nameof(index), index, "Index must not be negative and must be less than the Stack of this ItemSlot.");
        }

        #endregion
    }
}
