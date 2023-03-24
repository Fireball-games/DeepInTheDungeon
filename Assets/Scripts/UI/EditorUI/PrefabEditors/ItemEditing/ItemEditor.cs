using System.Collections.Generic;
using Scripts.Inventory.Inventories.Items;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class ItemEditor : MapPartsEditorWindowBase
    {
        private MapObjectList _itemList;
        private Title _itemTitle;
        
        private MapObject _selectedItem;

        private void Awake()
        {
            AssignComponents();
        }

        public override void Open()
        {
            body.SetActive(true);
            IEnumerable<MapObject> items = MapObject.GetAllItems();
            _itemList.Open(t.Get(Keys.AvailableItems), items, OnItemSelected);
            ItemCursor.Instance.Show();
        }

        private void OnItemSelected(MapObject selectedItem)
        {
            _selectedItem = selectedItem;
            ItemCursor.Instance.AddItem(selectedItem.DisplayPrefab);
            VisualizeComponents();
        }

        protected override void RemoveAndClose()
        {
            if (!_itemList) AssignComponents();
            
            body.SetActive(false);
            _itemList.SetActive(false);
            ItemCursor.Instance.Hide();
            
            VisualizeComponents();
        }
        
        private void VisualizeComponents()
        {
            _itemTitle.SetTitle(_selectedItem 
                ? t.GetItemText(_selectedItem.DisplayName) 
                : t.Get(Keys.NoItemSelected));
        }
        
        private void AssignComponents()
        {
            _itemList = GetComponentInChildren<MapObjectList>(true);
            _itemTitle = body.transform.Find("Background/Frame/Header/ItemTitle").GetComponent<Title>();
        }
    }
}