using Scripts.InventoryManagement.Inventories.Items;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectList : ListWindowBase<MapObject, MapObjectButton>
    {
        public static ItemPreview ItemPreview { get; private set; }

        protected void Awake()
        {
            ItemPreview = body.transform.Find("ItemPreview").GetComponent<ItemPreview>();
        }
        
        protected override string GetItemIdentification(MapObject item) => item.GetItemID();

        protected override void SetButton(MapObjectButton button, MapObject item)
        {
            base.SetButton(button, item);
            
            button.SetParentList(this);
        }

        public void ShowItemPreview(MapObject displayedItem) => ItemPreview.Show(displayedItem);

        public void HideItemPreview() => ItemPreview.Hide();
    }
}