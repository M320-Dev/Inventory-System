using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Samples.Demo.Runtime
{
    public static class ItemManager 
    {
        private static readonly HashSet<Item> _items = new();

        public static bool AddItem(Item item) 
        {
            return _items.Add(item);
        }
        public static bool RemoveItem(Item item)
        {
            return _items.Remove(item);
        }

        public static bool TryOverlapItem(Vector2 point, out Item overlapping)
        {
            return (overlapping = OverlapItem(point)) != null;
        }
        public static Item OverlapItem(Vector2 point)
        {
            return FindItem(item => item.collider2D ? item.collider2D.OverlapPoint(point) : false);
        }

        public static bool TryFindItem(out Item found, Predicate<Item> stopper)
        {
            return (found = FindItem(stopper)) != null;
        }
        public static Item FindItem(Predicate<Item> stopper)
        {
            Item found = null;

            EnumerateItems(item =>
            {
                if (stopper.Invoke(item))
                {
                    found = item;
                    return true;
                }
                return false;
            });

            return found;
        }
        public static void EnumerateItems(Predicate<Item> stopper) 
        {
            if (stopper == null) return;

            foreach (var item in _items)
            {
                if (stopper.Invoke(item)) return;
            }
        }
    }
}
