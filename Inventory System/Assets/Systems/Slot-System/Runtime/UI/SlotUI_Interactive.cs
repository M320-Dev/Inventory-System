using SlotSystem.Runtime.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotSystem.Runtime.UI
{
    public sealed class SlotUI_Interactive : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image m_interactiveImage;
        [SerializeField] private SlotUI m_slotUI;

        private void OnValidate()
        {
            SetUpInteractiveImage();
        }

        [ContextMenu("Set Up Interactive Image")]
        private void SetUpInteractiveImage() 
        {
            if (!m_interactiveImage) return;

            m_interactiveImage.sprite = null;
            m_interactiveImage.color = Color.clear;
            m_interactiveImage.raycastTarget = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CanStartDragging()) return;

            SetRaycastTarget(false);

            m_slotUI.ItemStackRT.SetParent(m_slotUI.ItemStackRT.root);
        }
        public void OnDrag(PointerEventData eventData)
        {
            m_slotUI.ItemStackRT.position = (Vector3)eventData.position;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            SetRaycastTarget(true);

            m_slotUI.ItemStackRT.SetParent(m_slotUI.transform);
            m_slotUI.ItemStackRT.SetAsFirstSibling();
            m_slotUI.ItemStackRT.localPosition = Vector3.zero;

            var hit = eventData.pointerCurrentRaycast;
            if (!hit.isValid) return;

            if (hit.gameObject.TryGetComponent(out SlotUI_Interactive slotUI_Interactive)) 
            {
                if (slotUI_Interactive == this) return;

                var result = SlotUtility.TransferOrSwap(m_slotUI.slot, slotUI_Interactive.m_slotUI.slot);

                if (result.type == SlotUtility.TransferOrSwapResult.Type.Swap)
                {
                    m_slotUI.UpdateUI();
                    slotUI_Interactive.m_slotUI.UpdateUI();
                }
            }
        }

        private bool CanStartDragging() 
        {
            return m_slotUI == null 
                || m_slotUI.slot == null 
                || m_slotUI.slot.ItemSO == null;
        }
        private void SetRaycastTarget(bool value) 
        {
            m_interactiveImage.raycastTarget = value;
            m_slotUI.SetItemStackRaycastTarget(value);
        }
    }
}
