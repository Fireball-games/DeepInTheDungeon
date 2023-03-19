using Scripts.Inventory.Inventories.Items;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectList : ListWindowBase<MapObject, MapObjectButton>
    {
        protected override string GetItemIdentification(MapObject item) => item.GetItemID();

        protected override void SetButton(MapObjectButton button, MapObject item)
        {
            base.SetButton(button, item);
            
            button.SetParentList(this);
        }
    }
}