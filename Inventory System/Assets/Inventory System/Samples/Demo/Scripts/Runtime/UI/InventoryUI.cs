using InventorySystem.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Demo.Runtime
{
    public sealed class InventoryUI : MonoBehaviour
    {
        [SerializeField] private SlotUI m_slotUIPrefab;
        [SerializeField] private RectTransform m_content;

        public IInventory inventory { get; private set; }

        private readonly List<SlotUI> _slotUIs = new();

        private int SlotCount => inventory != null ? inventory.SlotCount : 0;
        private int _previousSlotCount;

        private void Awake()
        {
            SetUpExistingSlotUIs();
        }

        private void SetUpExistingSlotUIs()
        {
            foreach (RectTransform element in m_content)
            {
                _slotUIs.Add(element.GetComponent<SlotUI>());
            }
        }

        public void SetInventory(IInventory inventory)
        {
            if (this.inventory != inventory)
            {
                if (this.inventory != null)
                {
                    this.inventory.ItemsAdded -= InventoryUpdated;
                    this.inventory.ItemsRemoved -= InventoryUpdated;
                }

                if (inventory != null)
                {
                    inventory.ItemsAdded += InventoryUpdated;
                    inventory.ItemsRemoved += InventoryUpdated;
                }

                this.inventory = inventory;

                UpdateUI();

                if (inventory != null) _previousSlotCount = inventory.SlotCount;
            }
        }

        private void InventoryUpdated(Dictionary<ISlot, List<IItem>> itemDictionary) => UpdateUI();

        [ContextMenu("Update UI")]
        private void UpdateUI()
        {
            if (inventory == null) return;

            Debug.Log("Update Inventory UI");

            float slotCountDiff = SlotCount - _previousSlotCount;

            if (slotCountDiff > 0)
            {
                if (_previousSlotCount == 0) _previousSlotCount = 1;
                for (int i = _previousSlotCount - 1; i < SlotCount; i++)
                {
                    if (i >= _slotUIs.Count) InstantiateSlotUI(inventory[i]);
                    else
                    {
                        _slotUIs[i].SetSlot(inventory[i]);
                        _slotUIs[i].gameObject.SetActive(true);
                    }
                }
            }
            else if (slotCountDiff < 0)
            {
                for (int i = _previousSlotCount - 1; i >= SlotCount; i--)
                {
                    _slotUIs[i].gameObject.SetActive(false);
                }
            }

            float minCount = Math.Min(SlotCount, _previousSlotCount);
            for (int i = 0; i < minCount; i++)
            {
                _slotUIs[i].SetSlot(inventory[i]);
            }
        }

        private void InstantiateSlotUI(ISlot slot) 
        {
            SlotUI slotUI = Instantiate(m_slotUIPrefab);
            slotUI.transform.SetParent(m_content);
            slotUI.SetSlot(slot);
            _slotUIs.Add(slotUI);
        }
    }
}
