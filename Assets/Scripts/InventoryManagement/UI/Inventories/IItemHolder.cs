using Scripts.InventoryManagement.Inventories.Items;

namespace Scripts.InventoryManagement.UI.Inventories
{
    /// <summary>
    /// Allows the `ItemTooltipSpawner` to display the right information.
    /// </summary>
    public interface IItemHolder
    {
        InventoryItem GetItem();
    }
}