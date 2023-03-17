using Scripts.Helpers;
using Scripts.Inventory.Inventories.Items;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectButton : ListButtonBase<MapObject>
    {
        private MapObjectList _parentList;
        
        public override void Set(MapObject item, UnityAction<MapObject> onClick, bool setSelectedOnClick = true)
        {
            base.Set(item, onClick, setSelectedOnClick);
            Text.text = displayedItem.DisplayName;
        }

        public void SetParentList(MapObjectList mapObjectList) => _parentList = mapObjectList;
    }
}