using System;
using UnityEngine;

namespace M320.ItemSystem
{
    public interface IItemSO 
    {
        Type ItemType { get; }

        #region Field Properties

        string ItemName { get; }
        string Description { get; }
        MaxStack MaxStack { get; }
        Sprite UISprite { get; }
        Color UISpriteColor { get; }
        IItem Prefab { get; }

        #endregion

        #region Properties

        bool IsStackable { get; }

        #endregion

        #region Method

        IItem Instantiate();

        #endregion
    }
    public interface IItemSO<TItem> : IItemSO
        where TItem : IItem
    {
        Type IItemSO.ItemType => typeof(TItem);

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
        where TItem : UnityEngine.Object, IItem
    {
        #region Fields

        [Header("Basic Info")]
        [SerializeField] private string m_itemName;
        [SerializeField, TextArea(4, 4)] private string m_description;
        [SerializeField] private MaxStack m_maxStack = MaxStack.Unstackable;
        [SerializeField] private Sprite m_uiSprite;
        [SerializeField] private Color m_uiSpriteColor = Color.white;
        [SerializeField] private TItem m_prefab;

        #endregion

        #region Field Properties

        public string ItemName => m_itemName;
        public string Description => m_description;
        public MaxStack MaxStack => m_maxStack;
        public Sprite UISprite => m_uiSprite;
        public Color UISpriteColor => m_uiSpriteColor;
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
