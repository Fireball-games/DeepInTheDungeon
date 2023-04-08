using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Scripts.UI.EditorUI.Components
{
    public abstract class ListWindowBase<T, TButton> : EditorWindowBase,
        IListWindow, IPointerEnterHandler, IPointerExitHandler
        where TButton : ListButtonBase<T>
    {
        [SerializeField] private Title title;
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject itemPrefab;
        public bool SetClickedItemSelected = true;
        public bool HasItems => Buttons is {Count: > 0};

        protected TButton LastAddedButton;

        private HashSet<TButton> Buttons;
        private UnityEvent<T> OnItemClicked { get; } = new();
        private UnityEvent OnCancelClicked { get; } = new();
        
        public PositionRotation OriginalCameraPositionRotation { get; set; }
        public int OriginalFloor { get; set; }
        private bool _navigatedAway;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            OriginalCameraPositionRotation = EditorCameraService.Instance.GetCameraTransformData();
            OriginalFloor = MapEditorManager.Instance.CurrentFloor;
            _navigatedAway = false;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_navigatedAway) return;
            
            MapEditorManager.Instance.SetFloor(OriginalFloor);
            EditorCameraService.Instance.MoveCameraTo(OriginalCameraPositionRotation);
        }

        public void Open(string listTitle, IEnumerable<T> items, UnityAction<T> onItemClicked, UnityAction onClose = null)
        {
            SetActive(true);
            title.SetTitle(listTitle);
            OnItemClicked.RemoveAllListeners();
            OnItemClicked.AddListener(onItemClicked);
            OnCancelClicked.RemoveAllListeners();

            if (onClose != null)
            {
                OnCancelClicked.AddListener(onClose);
            }

            listContent.gameObject.DismissAllChildrenToPool();

            Buttons ??= new HashSet<TButton>();
            Buttons.Clear();

            if (items == null) return;

            foreach (T item in items)
            {
                TButton newButton = ObjectPool.Instance.Get(itemPrefab, listContent).GetComponent<TButton>();

                SetButton(newButton, item);
                newButton.ParentList = this;
                
                Buttons.Add(newButton);
                LastAddedButton = newButton;
            }
        }
        
        public void SetNavigatedAway() => _navigatedAway = true;
        
        public void NavigateToClickedButtonItem()
        {
            OriginalFloor = MapEditorManager.Instance.CurrentFloor;
            OriginalCameraPositionRotation = EditorCameraService.Instance.GetCameraTransformData();
        }

        /// <summary>
        /// Button set is separated from Open method to allow for custom button settings.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="item"></param>
        protected virtual void SetButton(TButton button, T item)
        {
            button.Set(item, OnItemClicked_internal, SetClickedItemSelected);
        }

        public void DeselectButtons() => Buttons?.ForEach(b => b.SetSelected(false));

        public void SetButtonsInteractable(bool isInteractable) => Buttons.ForEach(b => b.SetInteractable(isInteractable));

        public void Close() => SetActive(false);

        protected abstract string GetItemIdentification(T item);

        protected virtual void OnItemClicked_internal(T item)
        {
            if (SetClickedItemSelected)
            {
                Buttons.ForEach(button => button.SetSelected(GetItemIdentification(button.displayedItem) == GetItemIdentification(item)));
            }

            OnItemClicked.Invoke(item);
        }

        protected void OnCancelClicked_internal()
        {
            SetActive(false);
        }
    }
}