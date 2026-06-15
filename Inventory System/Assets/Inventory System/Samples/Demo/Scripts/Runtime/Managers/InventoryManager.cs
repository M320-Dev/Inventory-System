using InventorySystem.Runtime;
using UnityEngine;

namespace InventorySystem.Demo.Runtime
{
    public sealed class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InventoryManagerInput m_input;
        [SerializeField, Min(1)] private int m_slotCount = 12;
        [SerializeField] private InventoryUI m_ui;

        private Inventory _inventory;

        private void Awake()
        {
            _inventory = new(m_slotCount);
            m_ui.SetInventory(_inventory);
        }

        private void OnEnable()
        {
            m_input.Take += Take;
            m_input.Drop += Drop;
        }
        private void OnDisable()
        {
            m_input.Take -= Take;
            m_input.Drop -= Drop;
        }

        private void Take(Vector2 point) 
        {
            if (ItemManager.TryOverlapItem(point, out Item overlapping)) 
            {
                _inventory.AddItem(overlapping);
                overlapping.gameObject.SetActive(false);
            }
        }
        private void Drop(Vector2 point)
        {
            if (!ItemManager.OverlapItem(point))
            {
                if (_inventory.TryRemoveItem<ItemSO>(out IItem removed)) 
                {
                    Item dropped = (Item)removed;
                    dropped.transform.position = point;
                    dropped.gameObject.SetActive(true);
                }
            }
        }
    }
}
