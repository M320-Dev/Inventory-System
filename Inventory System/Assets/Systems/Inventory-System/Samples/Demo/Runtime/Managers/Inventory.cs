using M320.InventorySystem.UI;
using M320.ItemSystem;
using M320.SlotSystem;

using UnityEngine;

namespace M320.InventorySystem.Samples.Demo
{
    public sealed class Inventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int m_slotCount = 12;
        [SerializeField] private InventoryUI m_inventoryUI;

        private Inventory<Slot> _inventory;

        private void Awake()
        {
            _inventory = InventoryFactory.EmptySlots<Slot>(m_slotCount);
        }
        private void Start()
        {
            m_inventoryUI.SetInventory(_inventory);
        }

        public bool AddItem(Item item)
        {
            if (_inventory.AddItem(item))
            {
                item.gameObject.SetActive(false);
                return true;
            }
            return false;
        }
        public bool TryRemoveItem(out Item item)
        {
            return (item = RemoveItem()) != null;
        }
        public Item RemoveItem()
        {
            if (_inventory.TryRemoveItem<ItemSO>(out IItem removed))
            {
                Item _removed = (Item)removed;
                _removed.gameObject.SetActive(true);
                return _removed;
            }
            return null;
        }
    }
}
