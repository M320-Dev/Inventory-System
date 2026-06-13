using UnityEngine;

namespace InventorySystem.Runtime
{
    public interface IItem 
    {
        IItemSO ItemSO { get; }
    }
    public interface IItem<TItemSO> : IItem
        where TItemSO : IItemSO
    {
        IItemSO IItem.ItemSO => ItemSO;
        new TItemSO ItemSO { get; }
    }


    public abstract class Item<TItemSO> : MonoBehaviour, IItem<TItemSO>
        where TItemSO : IItemSO
    {
        [SerializeField] private TItemSO m_itemSO;

        public TItemSO ItemSO => m_itemSO;
    }
}
