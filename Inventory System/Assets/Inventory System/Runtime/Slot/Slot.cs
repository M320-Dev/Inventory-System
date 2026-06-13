using System;
using System.Collections;
using System.Collections.Generic;

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

        int CurrentSize { get; }
        int MaxSize { get; }

        bool IsEmpty { get; }
        bool IsFull { get; }

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

        bool TryRemoveItem(out IItem item);
        IItem RemoveItem();
        List<IItem> RemoveItems(int stack);

        #endregion

        #region Contains

        bool ContainsItem(IItem item);

        #endregion
    }

    public sealed class Slot : ISlot
    {
        #region Field

        private IItem[] _items;

        #endregion

        #region Field Property

        public IItemSO ItemSO { get; private set; }

        #endregion

        #region Indexer

        public IItem this[int index] => GetItem(index);

        #endregion

        #region Properties

        public int CurrentSize { get; private set; }
        public int MaxSize => _items.Length;

        public bool IsEmpty => CurrentSize <= 0;
        public bool IsFull => CurrentSize >= MaxSize;

        #endregion

        #region Events

        public event Action<IReadOnlyList<IItem>> ItemsAdded;
        public event Action<IReadOnlyList<IItem>> ItemsRemoved;

        #endregion

        #region Get

        public IItem GetItem(int index)
        {
            TryThrowNotWithinCurrentSize(index);

            return _items[index];
        }

        #endregion

        #region Clear

        public void ClearItems() 
        {
            Array.Clear(_items, 0, CurrentSize);
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

                _items[CurrentSize++] = item;

                ItemsAdded?.Invoke(new IItem[] { item });

                return true;
            }
            else if (!IsFull && ItemSO == item.ItemSO)
            {
                _items[CurrentSize++] = item;

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

                    _items[CurrentSize++] = item;

                    addedItems.Add(item);
                }
            }

            while (!IsFull && enumerator.MoveNext())
            {
                IItem item = enumerator.Current;

                if (item != null && !ContainsItem(item) && ItemSO == item.ItemSO)
                {
                    _items[CurrentSize++] = item;

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

            IItem removedItem = _items[--CurrentSize];

            if (IsEmpty) Array.Resize(ref _items, 0);
            else _items[CurrentSize] = null;

            ItemsRemoved?.Invoke(new IItem[] { removedItem });

            return removedItem;
        }
        public List<IItem> RemoveItems(int stack)
        {
            if (IsEmpty || stack < 1) return new();

            stack = Math.Min(stack, CurrentSize);
            int startIndex = CurrentSize - stack;

            List<IItem> removedItems = new();

            for (int i = CurrentSize - 1; i >= startIndex; i--)
            {
                IItem removedItem = _items[--CurrentSize];

                removedItems.Add(removedItem);

                if (IsEmpty) Array.Resize(ref _items, 0);
                else _items[CurrentSize] = null;
            }

            ItemsRemoved?.Invoke(removedItems);

            return removedItems;
        }

        #endregion

        #region Contains

        public bool ContainsItem(IItem item)
        {
            for (int i = 0; i < CurrentSize; i++)
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

        private bool WithinCurrentSize(int index) 
        {
            return index >= 0 && index < CurrentSize;
        }
        private void TryThrowNotWithinCurrentSize(int index)
        {
            if (!WithinCurrentSize(index)) throw new ArgumentOutOfRangeException(
                nameof(index), index, "Index must not be negative and must be less than the CurrentSize of this ItemSlot.");
        }

        #endregion
    }
}
