using Scripts.Helpers;
using Scripts.Inventory.Inventories.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class MapObjectButton : ListButtonBase<MapObject>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite normalFrame;
        [SerializeField] private Sprite highlightedFrame;
        [SerializeField] private Sprite selectedFrame;
        
        private Image _frame;
        private MapObjectList _parentList;
        private Image _itemImage;

        public override void Set(MapObject item, UnityAction<MapObject> onClick, bool setSelectedOnClick = true)
        {
            base.Set(item, onClick, setSelectedOnClick);
            
            _itemImage ??= transform.Find("Button/Frame/ItemImage").GetComponent<Image>();
            _itemImage.sprite = item.Icon;
            _itemImage.color = Colors.FullOpaqueWhite;
        }

        public override void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            _frame.sprite = isSelected ? selectedFrame : normalFrame;
        }

        public void SetParentList(MapObjectList mapObjectList) => _parentList = mapObjectList;
        public void OnPointerEnter(PointerEventData eventData)
        {
            _frame.sprite = highlightedFrame;
            
            if (_parentList)
            {
                _parentList.ShowItemPreview(displayedItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsSelected) _frame.sprite = normalFrame;
            
            if (_parentList)
            {
                _parentList.HideItemPreview();
            }
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            _frame = transform.Find("Button/Frame").GetComponent<Image>();
        }
    }
}