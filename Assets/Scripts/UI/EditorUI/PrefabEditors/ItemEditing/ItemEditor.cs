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
using Scripts.MapEditor.Services;
using Scripts.System.MonoBases;
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
        
        public static DetailCursorSetup AddCursorSetup { get; private set; }
        public static DetailCursorSetup EditCursorSetup { get; private set; }
        public static DetailCursorSetup RemoveCursorSetup { get; private set; }

        private MapObjectList _itemList;
        public static ItemPreview ItemPreview { get; private set; }
        private static ItemCursor ItemCursor => ItemCursor.Instance;
        private Title _itemTitle;
        private Button _saveButton;
        private Button _cancelButton;
        
        private MapObject _selectedItemToSpawn;
        private MapObjectInstance _selectedItemInstance;
        private MapObjectConfiguration _selectedItemConfiguration;

        private bool _areItemsChanged;

        public Func<SpriteRenderer, Tween> DetailTweenFunc { get; private set; }

        private void Awake()
        {
            AssignComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorEvents.OnAddItemToMap.AddListener(OnAddItemToMap);
            EditorEvents.OnMapSaved += OnMapSaved;
            ItemsMouseService.OnMouseEnterMapObject.AddListener(OnMouseEnterMapObject);
            ItemsMouseService.OnMouseExitMapObject.AddListener(OnMouseExitMapObject);
            ItemsMouseService.OnObjectActivated.AddListener(OnObjectActivated);
            ItemsMouseService.OnObjectDeactivated.AddListener(OnObjectDeactivated);
            ItemsMouseService.OnObjectChanged.AddListener(OnObjectChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorEvents.OnAddItemToMap.RemoveListener(OnAddItemToMap);
            EditorEvents.OnMapSaved -= OnMapSaved;
            ItemsMouseService.OnMouseEnterMapObject.RemoveListener(OnMouseEnterMapObject);
            ItemsMouseService.OnMouseExitMapObject.RemoveListener(OnMouseExitMapObject);
            ItemsMouseService.OnObjectActivated.RemoveListener(OnObjectActivated);
            ItemsMouseService.OnObjectDeactivated.RemoveListener(OnObjectDeactivated);
            ItemsMouseService.OnObjectChanged.RemoveListener(OnObjectChanged);
        }

        public override void Open()
        {
            body.SetActive(true);
            IEnumerable<MapObject> items = MapObject.GetAllItems();
            _itemList.Open(t.Get(Keys.AvailableItems), items, OnItemSelected);
            MapEditorManager.SetEditMode(EEditMode.Edit);
        }
        
        private static void OnMouseEnterMapObject(MapObjectInstance mapObject) => ItemPreview.Show(mapObject.Item);

        private static void OnMouseExitMapObject(MapObjectInstance mapObject) => ItemPreview.Hide();
        
        private void OnObjectActivated(MapObjectInstance activatedInstance)
        {
            _selectedItemInstance = activatedInstance;
            VisualizeComponents();
        }

        private void OnObjectDeactivated(MapObjectInstance deactivatedInstance)
        {
            _selectedItemInstance = null;
            VisualizeComponents();
        }

        private void OnObjectChanged(MapObjectInstance changedInstance) => SetEdited(true);

        private void OnMapSaved() => SetEdited(false);

        private void OnItemSelected(MapObject selectedItem)
        {
            MapEditorManager.SetEditMode(EEditMode.Add);
            _selectedItemToSpawn = selectedItem;
            _selectedItemConfiguration = MapObjectConfiguration.Create(selectedItem);
            ItemCursor.ShowAndFollowMouse(selectedItem.DisplayPrefab)
                .SetDetailImage(addCursorSetup.Image, Colors.Get(addCursorSetup.Color), DetailTweenFunc);
            
            VisualizeComponents();
        }
        
        private void OnAddItemToMap(Vector3 clickPosition)
        {
            if (!_selectedItemToSpawn) return;
            
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
            
            MapObject tempSelectedItem = _selectedItemToSpawn;
            
            _itemList.SetButtonsInteractable(false);
            await MapBuilder.RebuildItems();
            _itemList.SetButtonsInteractable(true);
            ItemCursor.Hide();

            if (tempSelectedItem) OnItemSelected(tempSelectedItem);

            SetEdited(false);
        }

        protected override async void RemoveAndClose()
        {
            if (_areItemsChanged && await MapEditorManager.CheckToSaveMapChanges() is DialogBase.EConfirmResult.Cancel)
            {
                RemoveChanges();
            }
            
            if (!_itemList) AssignComponents();
            
            body.SetActive(false);
            _itemList.DeselectButtons();
            _itemList.Close();
            ItemCursor.Hide();
            _selectedItemToSpawn = null;
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
            _itemTitle.SetTitle(_selectedItemToSpawn 
                ? t.GetItemText(_selectedItemToSpawn.DisplayName) 
                : t.Get(Keys.NoItemSelected));
            
            _saveButton.SetText(t.Get(Keys.Save));
            _saveButton.gameObject.SetActive(_areItemsChanged);
            _cancelButton.SetText(t.Get(Keys.Cancel));
            _cancelButton.gameObject.SetActive(_areItemsChanged);
        }
        
        private void AssignComponents()
        {
            ItemPreview = GetComponentInChildren<ItemPreview>(true);
            
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
            
            DetailTweenFunc = image => image.DOFade(0.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
            
            addCursorSetup.DetailTweenFunc = DetailTweenFunc;
            AddCursorSetup = addCursorSetup;
            
            editCursorSetup.DetailTweenFunc = DetailTweenFunc;
            EditCursorSetup = editCursorSetup;
            
            removeCursorSetup.DetailTweenFunc = DetailTweenFunc;
            RemoveCursorSetup = removeCursorSetup;
        }
    }
}