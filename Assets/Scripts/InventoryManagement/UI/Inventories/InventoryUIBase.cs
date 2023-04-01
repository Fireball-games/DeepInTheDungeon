using Scripts.System.MonoBases;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public class InventoryUIBase : UIElementBase
    {
        public void ToggleOpen() => SetActive(!body.activeSelf);
        
        public void Close() => SetActive(false);
    }
}