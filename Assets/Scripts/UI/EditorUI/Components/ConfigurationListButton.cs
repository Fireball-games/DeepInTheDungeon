﻿using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static Scripts.UI.IconStore;
using static Scripts.Enums;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationListButton : ListButtonBase<PrefabConfiguration>, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isCameraStayingOnNavigatedPosition;
        
        private PositionRotation _originalCameraTransformData;
        private int _originalFloor;

        private EditorUIManager UIManager => EditorUIManager.Instance;
        private Cursor3D Cursor3D => UIManager ? UIManager.Cursor3D : null;
        private EditorCameraService CameraService => EditorCameraService.Instance;

        public void Set(PrefabConfiguration item, UnityAction<PrefabConfiguration> onClick, bool setSelectedOnClick = true, bool _isCameraStayingOnNavigatedPosition = false)
        {
            base.Set(item, onClick, setSelectedOnClick);

            isCameraStayingOnNavigatedPosition = _isCameraStayingOnNavigatedPosition;

            GameObject instancedPrefab = GameManager.Instance.MapBuilder
                .GetPrefabByGuid(item.SpawnPrefabOnBuild ? item.Guid : item.OwnerGuid);

            if (item.PrefabType is EPrefabType.TriggerOnWall) AddIcon(EIcon.Trigger);
            if (item.PrefabType is EPrefabType.TriggerReceiver) AddIcon(EIcon.TriggerReceiver);
            if (!item.SpawnPrefabOnBuild) AddIcon(EIcon.Embedded);

            if (instancedPrefab && instancedPrefab.GetBody()) AddIcon(EIcon.Wall);
            if (item is WallConfiguration configuration && configuration.HasPath()) AddIcon(EIcon.Move);

            Text.text = displayedItem.PrefabName;
        }

        protected override void OnClick_internal()
        {
            base.OnClick_internal();

            if (isCameraStayingOnNavigatedPosition)
            {
                _originalCameraTransformData =
                    new PositionRotation(CameraService.MoveCameraToPrefab(Vector3Int.RoundToInt(displayedItem.TransformData.Position)),
                        Quaternion.Euler(Vector3.zero));
                _originalFloor = Mathf.RoundToInt(-displayedItem.TransformData.Position.y);
            }
            else
            {
                OnPointerExit(null);
            }
            
            Cursor3D.Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Quaternion rotation = displayedItem.TransformData.Rotation;
            Vector3 position = displayedItem.TransformData.Position;
            
            Vector3 cursorScale = EditorPrefabScaleOverrides.Get(displayedItem.PrefabName, out Vector3 scale) 
                ? scale
                : UIManager.OpenedEditor.GetCursor3DScale();
            
            Cursor3D.ShowAt(position, cursorScale, rotation);

            OnMouseOver();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor3D.Hide();
        }

        private void OnMouseOver()
        {
            if (EditorMouseService.Instance.IsManipulatingCameraPosition) return;
            
            UIManager.OpenedEditor.MoveCameraToPrefab(Vector3Int.RoundToInt(displayedItem.TransformData.Position));
            ParentList.SetNavigatedAway();
        }
    }
}