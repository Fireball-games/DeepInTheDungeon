using Scripts.Player;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public class EquipmentUI : InventoryUIBase
    {
        private bool _lookModeOnBeforeInventory;
        
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
                _lookModeOnBeforeInventory = PlayerCamera.IsLookModeOn;
                PlayerCamera.IsLookModeOn = false;
            }
            else
            {
                PlayerCamera.IsLookModeOn = _lookModeOnBeforeInventory;
            }
        }
    }
}