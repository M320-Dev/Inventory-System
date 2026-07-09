using UnityEngine;

namespace M320.InventorySystem.Samples.Demo
{
    public sealed class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InventoryManagerInput m_input;
        [SerializeField] private Inventory m_inventory;

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
                m_inventory.AddItem(overlapping);
            }
        }
        private void Drop(Vector2 point)
        {
            if (!ItemManager.OverlapItem(point))
            {
                if (m_inventory.TryRemoveItem(out Item removed)) 
                {
                    removed.transform.position = point;
                }
            }
        }
    }
}
