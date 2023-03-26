using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class ItemEditor : MapPartsEditorWindowBase
    {
        [SerializeField] private DetailCursorSetup addCursorSetup;
        [SerializeField] private DetailCursorSetup editCursorSetup;
        [SerializeField] private DetailCursorSetup removeCursorSetup;
        
        private MapObjectList _itemList;
        private Title _itemTitle;
        private Button _saveButton;
        private Button _cancelButton;
        
        private MapObject _selectedItem;
        private MapObjectConfiguration _selectedItemConfiguration;

        private bool _areItemsChanged;
        
        private Func<SpriteRenderer, Tween> _tweenFunc;

        private void Awake()
        {
            AssignComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorEvents.OnAddItemToMap.AddListener(OnAddItemToMap);
            EditorEvents.OnMapSaved += OnMapSaved;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            EditorEvents.OnAddItemToMap.RemoveListener(OnAddItemToMap);
            EditorEvents.OnMapSaved -= OnMapSaved;
        }

        public override void Open()
        {
            body.SetActive(true);
            IEnumerable<MapObject> items = MapObject.GetAllItems();
            _itemList.Open(t.Get(Keys.AvailableItems), items, OnItemSelected);
            MapEditorManager.SetEditMode(EEditMode.Edit);
        }
        
        private void OnMapSaved() => SetEdited(false);

        private void OnItemSelected(MapObject selectedItem)
        {
            MapEditorManager.SetEditMode(EEditMode.Add);
            _selectedItem = selectedItem;
            _selectedItemConfiguration = MapObjectConfiguration.Create(selectedItem);
            ItemCursor.Instance.Show(selectedItem.DisplayPrefab)
                .SetDetailImage(addCursorSetup.Image, Colors.GetColor(addCursorSetup.Color), _tweenFunc);
            
            VisualizeComponents();
        }
        
        private void OnAddItemToMap(Vector3 clickPosition)
        {
            if (!_selectedItem) return;
            
            _selectedItemConfiguration.PositionRotation.Position = clickPosition;
            
            MapBuilder.SpawnItem(_selectedItemConfiguration);
            
            SetEdited(true);
        }
        
        private void Save()
        {
            MapEditorManager.SaveMap();
            SetEdited(false);
        }
        
        private async void RemoveChanges()
        {
            if (!_areItemsChanged) return;
            
            _itemList.SetButtonsInteractable(false);
            await MapBuilder.RebuildItems();
            _itemList.SetButtonsInteractable(true);
            OnItemSelected(_selectedItem);
            
            SetEdited(false);
        }

        protected override async void RemoveAndClose()
        {
            if (_areItemsChanged)
            {
                await MapEditorManager.CheckToSaveMapChanges();
            }
            
            if (!_itemList) AssignComponents();
            
            body.SetActive(false);
            _itemList.DeselectButtons();
            _itemList.Close();
            ItemCursor.Instance.Hide();
            _selectedItem = null;
            _selectedItemConfiguration = null;
            MapEditorManager.SetEditMode(EEditMode.Normal);
            
            VisualizeComponents();
        }

        private void SetEdited(bool isEdited)
        {
            _areItemsChanged = isEdited;
            
            if (isEdited)
            {
                EditorEvents.TriggerOnMapLayoutChanged();
            }
            
            VisualizeComponents();
        }
        
        private void VisualizeComponents()
        {
            _itemTitle.SetTitle(_selectedItem 
                ? t.GetItemText(_selectedItem.DisplayName) 
                : t.Get(Keys.NoItemSelected));
            
            _saveButton.SetText(t.Get(Keys.Save));
            _saveButton.gameObject.SetActive(_areItemsChanged);
            _cancelButton.SetText(t.Get(Keys.Cancel));
            _cancelButton.gameObject.SetActive(_areItemsChanged);
        }
        
        private void AssignComponents()
        {
            _itemList = GetComponentInChildren<MapObjectList>(true);
            Transform frame = body.transform.Find("Background/Frame");
            _itemTitle = frame.Find("Header/ItemTitle").GetComponent<Title>();
            
            Transform content = frame.Find("ScrollingContent/Viewport/Content");
            Transform buttons = frame.Find("Buttons");
            
            _saveButton = buttons.Find("SaveDelete/SaveButton").GetComponent<Button>();
            _saveButton.onClick.AddListener(Save);
            _saveButton.SetTextColor(Colors.Positive);
            
            _cancelButton = buttons.Find("CancelClose/CancelButton").GetComponent<Button>();
            _cancelButton.onClick.AddListener(RemoveChanges);
            _cancelButton.SetTextColor(Colors.Negative);
            
            _tweenFunc = image => image.DOFade(0.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
        }
    }
}