using Scripts.Helpers;
using Scripts.Inventory.Inventories.Items;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectButton : ListButtonBase<MapObject>, IPointerEnterHandler, IPointerExitHandler
    {
        private MapObjectList _parentList;
        private Image _itemImage;
        
        public override void Set(MapObject item, UnityAction<MapObject> onClick, bool setSelectedOnClick = true)
        {
            base.Set(item, onClick, setSelectedOnClick);
            
            _itemImage ??= transform.Find("Button/Frame/ItemImage").GetComponent<Image>();
            _itemImage.sprite = item.Icon;
            _itemImage.color = Colors.FullOpaqueWhite;
        }

        public void SetParentList(MapObjectList mapObjectList) => _parentList = mapObjectList;
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Show Tooltip
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Hide Tooltip
        }
    }
}