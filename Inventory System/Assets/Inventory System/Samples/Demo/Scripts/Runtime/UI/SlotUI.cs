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
                    this.slot.ItemsAdded -= SlotUpdated;
                    this.slot.ItemsRemoved -= SlotUpdated;
                }

                if (slot != null)
                {
                    slot.ItemsAdded += SlotUpdated;
                    slot.ItemsRemoved += SlotUpdated;
                }

                this.slot = slot;

                UpdateUI();
            }
        }

        private void SlotUpdated(IReadOnlyList<IItem> items) => UpdateUI();

        [ContextMenu("Update UI")]
        private void UpdateUI()
        {
            if (slot == null) return;

            Debug.Log("Update Slot UI");

            UpdateItemImage();
            UpdateStackTMP();
        }
        private void UpdateItemImage()
        {
            if (!m_itemImage) return;

            m_itemImage.enabled = Enabled();
            if (!m_stackTMP.enabled) return;

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
