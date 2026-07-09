using M320.ItemSystem;
using UnityEngine;

namespace M320.InventorySystem.Samples.Demo
{ 
    public sealed class Item : Item<ItemSO> 
    {
        [SerializeField] private Collider2D m_collider2D;

        public new Collider2D collider2D => m_collider2D;

        private void Awake()
        {
            ItemManager.AddItem(this);
        }
        private void OnDestroy()
        {
            ItemManager.RemoveItem(this);
        }
    }
}
