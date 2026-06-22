using InventorySystem.Runtime.Core;
using InventorySystem.Runtime.UI;
using ItemSystem.Runtime;
using SlotSystem.Runtime.Core;
using UnityEngine;

namespace InventorySystem.Samples.Demo.Runtime
{
    public sealed class Inventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int m_slotCount = 12;
        [SerializeField] private InventoryUI m_inventoryUI;

        private Inventory<Slot> _inventory;

        private void Awake()
        {
            _inventory = InventoryFactory.ParameterlessSlotConstructor<Slot>(m_slotCount);
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
