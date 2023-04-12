using Scripts.Player;
using Scripts.System;
using Scripts.UI;
using UnityEngine.EventSystems;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public class EquipmentUI : InventoryUIBase, IDraggableWindow
    {
        private bool _lookModeOnBeforeInventory;
        
        private static GameManager GameManager => GameManager.Instance;
        private static PlayerCameraController PlayerCamera => PlayerCameraController.Instance;

        public override void OnInitialize()
        {
            // nothing needed now
        }

        protected override void SetTitle()
        {
        }

        public override void ToggleOpen()
        {
            base.ToggleOpen();
            
            if (gameObject.activeSelf)
            {
                if (GameManager.Instance.CurrentCharacterProfile != null)
                {
                    RectTransform.position = GameManager.Instance.CurrentCharacterProfile.inventoryPosition;
                }
                
                _lookModeOnBeforeInventory = PlayerCamera.IsLookModeOn;
                PlayerCamera.IsLookModeOn = false;
            }
            else
            {
                PlayerCamera.IsLookModeOn = _lookModeOnBeforeInventory;
            }
        }
        
        public void OnDragStart(PointerEventData eventData)
        {
            // Nothing to do here
        }

        public void OnDragEnd(PointerEventData eventData)
        {
            if (GameManager.CurrentCharacterProfile != null)
            {
                GameManager.CurrentCharacterProfile.inventoryPosition = RectTransform.position;
            }
        }
    }
}