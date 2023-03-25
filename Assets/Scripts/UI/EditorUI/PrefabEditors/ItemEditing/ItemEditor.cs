﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Building.ItemSpawning;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Inventory.Inventories.Items;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using UnityEngine;
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
        
        private MapObject _selectedItem;
        private MapObjectConfiguration _selectedItemConfiguration;
        
        private Func<SpriteRenderer, Tween> _tweenFunc;

        private void Awake()
        {
            AssignComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorEvents.OnAddItemToMap.AddListener(OnAddItemToMap);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            EditorEvents.OnAddItemToMap.RemoveListener(OnAddItemToMap);
        }

        public override void Open()
        {
            body.SetActive(true);
            IEnumerable<MapObject> items = MapObject.GetAllItems();
            _itemList.Open(t.Get(Keys.AvailableItems), items, OnItemSelected);
            MapEditorManager.SetEditMode(EEditMode.Edit);
        }

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
        }

        protected override void RemoveAndClose()
        {
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
            
            _tweenFunc = image => image.DOFade(0.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false)
                .Play();
        }
    }
}