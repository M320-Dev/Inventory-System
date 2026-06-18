using System;
using UnityEngine;

namespace ItemSystem.Runtime
{
    public interface IItem
    {
        Type ItemSOType { get; }

        IItemSO ItemSO { get; }
    }
    public interface IItem<TItemSO> : IItem
        where TItemSO : IItemSO
    {
        Type IItem.ItemSOType => typeof(TItemSO);

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
