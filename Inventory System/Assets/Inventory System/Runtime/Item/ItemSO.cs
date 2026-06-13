using UnityEngine;

namespace InventorySystem.Runtime
{
    public interface IItemSO 
    {
        #region Field Properties

        string ItemName { get; }
        string Description { get; }
        int MaxStack { get; }
        IItem Prefab { get; }

        #endregion

        #region Method

        IItem Instantiate();

        #endregion
    }
    public interface IItemSO<TItem> : IItemSO
        where TItem : IItem
    {
        #region Field Propertie

        IItem IItemSO.Prefab => Prefab;
        new TItem Prefab { get; }

        #endregion

        #region Method

        IItem IItemSO.Instantiate() => Instantiate();
        new TItem Instantiate();

        #endregion
    }

    public abstract class ItemSO<TItem> : ScriptableObject, IItemSO<TItem>
        where TItem : Object, IItem
    {
        #region Fields

        [Header("Basic Info")]
        [SerializeField] private string m_itemName;
        [SerializeField] private string m_description;
        [SerializeField, Min(1)] private int m_maxStack = 1;
        [SerializeField] private TItem m_prefab;

        #endregion

        #region Field Properties

        public string ItemName => m_itemName;
        public string Description => m_description;
        public int MaxStack => m_maxStack;
        public TItem Prefab => m_prefab;

        #endregion

        #region Properties

        public bool IsStackable => m_maxStack > 1;

        #endregion

        #region Method

        public TItem Instantiate() => Instantiate(m_prefab);

        #endregion
    }
}
