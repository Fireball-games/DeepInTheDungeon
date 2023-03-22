using Scripts.Inventory.Inventories.Items;
using Scripts.UI.EditorUI.PrefabEditors;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectList : ListWindowBase<MapObject, MapObjectButton>
    {
        private ItemPreview _itemPreview;
        
        protected void Awake()
        {
            _itemPreview = body.transform.Find("ItemPreview").GetComponent<ItemPreview>();
        }
        
        protected override string GetItemIdentification(MapObject item) => item.GetItemID();

        protected override void SetButton(MapObjectButton button, MapObject item)
        {
            base.SetButton(button, item);
            
            button.SetParentList(this);
        }

        public void ShowItemPreview(MapObject displayedItem) => _itemPreview.Show(displayedItem);

        public void HideItemPreview() => _itemPreview.Hide();
    }
}