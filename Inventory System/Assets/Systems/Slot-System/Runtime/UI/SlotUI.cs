using M320.ItemSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace M320.SlotSystem.UI
{
    public sealed class SlotUI : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private Image m_slotImage;
        [SerializeField] private RectTransform m_itemStackRT;
        [SerializeField] private Image m_itemImage;
        [SerializeField] private TextMeshProUGUI m_stackTMP;

        #endregion

        #region Inspector Field Properties

        public Image SlotImage => m_slotImage;
        public RectTransform ItemStackRT => m_itemStackRT;
        public Image ItemImage => m_itemImage;
        public TextMeshProUGUI StackTMP => m_stackTMP;

        #endregion

        public ISlot slot { get; private set; }

        public void SetSlot(ISlot slot)
        {
            if (this.slot != slot)
            {
                if (this.slot != null)
                {
                    this.slot.ItemsAdded -= ItemsAdded;
                    this.slot.ItemsRemoved -= ItemsRemoved;
                }

                if (slot != null)
                {
                    slot.ItemsAdded += ItemsAdded;
                    slot.ItemsRemoved += ItemsRemoved;
                }

                this.slot = slot;

                UpdateUI();
            }
        }

        private void ItemsAdded(IReadOnlyList<IItem> items) => UpdateUI();
        private void ItemsRemoved(IItemSO previousItemSO, IReadOnlyList<IItem> removedItems) => UpdateUI();

        [ContextMenu("Update UI")]
        public void UpdateUI()
        {
            UpdateItemImage();
            UpdateStackTMP();
        }
        private void UpdateItemImage()
        {
            if (!m_itemImage) return;

            m_itemImage.enabled = Enabled();
            if (!m_itemImage.enabled) return;

            m_itemImage.sprite = slot.ItemSO.UISprite;
            m_itemImage.color = slot.ItemSO.UISpriteColor;
        }
        private void UpdateStackTMP()
        {
            if (!m_stackTMP) return;

            m_stackTMP.enabled = Enabled();
            if (!m_stackTMP.enabled) return;

            m_stackTMP.text = slot.Stack > 1 ? slot.Stack.ToString() : "";
        }

        private bool Enabled()
        {
            return slot != null && slot.ItemSO != null;
        }

        public void SetItemStackRaycastTarget(bool value)
        {
            m_itemImage.raycastTarget = value;
            m_stackTMP.raycastTarget = value;
        }
    }
}
