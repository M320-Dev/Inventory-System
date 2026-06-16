using InventorySystem.Runtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.Demo.Runtime
{
    public sealed class SlotUI : MonoBehaviour
    {
        [SerializeField] private Image m_itemImage;
        [SerializeField] private TextMeshProUGUI m_stackTMP;

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
        private void UpdateUI()
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
    }
}
