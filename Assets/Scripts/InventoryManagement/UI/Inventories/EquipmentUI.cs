using Scripts.Localization;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public class EquipmentUI : InventoryUIBase
    {
        protected override void SetTitle() => title.text = t.Get(Keys.EquipmentTitle);
    }
}