using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.Helpers.Extensions;
using Scripts.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static Scripts.UI.IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabListButton : ListButtonBase<PrefabBase>, IPointerEnterHandler, IPointerExitHandler
    {
        private UnityEvent<GameObject> OnMouseEnter { get; } = new();
        private UnityEvent OnMouseExit { get; } = new();
        
        public override void Set(PrefabBase item, UnityAction<PrefabBase> onClick, bool setSelectedOnClick = true)
        {
            base.Set(item, onClick, setSelectedOnClick);

            if (item.gameObject.GetBody()) AddIcon(EIcon.Wall);
            
            Text.text = displayedItem.gameObject.name;
            
            switch (item)
            {
                case IMovementWall:
                {
                    AddIcon(EIcon.Move);
                    break;
                }
                case IWallWithTrigger:
                {
                    if (item.GetComponent<TriggerReceiver>()) AddIcon(EIcon.TriggerReceiver);
                    if (item.GetComponentInChildren<Trigger>()) AddIcon(EIcon.Trigger);
                    break;
                }
            }
        }
        
        public void SetMousePointerMethods(UnityAction<GameObject> onEnter, UnityAction onExit)
        {
            OnMouseEnter.RemoveAllListeners();
            OnMouseEnter.AddListener(onEnter);
            
            OnMouseExit.RemoveAllListeners();
            OnMouseExit.AddListener(onExit);
        }

        public void OnPointerEnter(PointerEventData eventData) => OnMouseEnter.Invoke(displayedItem.gameObject);

        public void OnPointerExit(PointerEventData eventData) => OnMouseExit.Invoke();
    }
}