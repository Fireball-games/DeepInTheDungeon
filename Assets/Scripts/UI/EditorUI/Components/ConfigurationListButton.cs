using System.Collections;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationListButton : ListButtonBase<PrefabConfiguration>, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isCameraStayingOnNavigatedPosition;
        
        private readonly WaitForSecondsRealtime _startNavigatingDelay = new(0.5f);
        private PositionRotation _originalCameraTransformData;
        private int _originalFloor;

        private EditorUIManager UIManager => EditorUIManager.Instance;
        private Cursor3D Cursor3D => UIManager ? UIManager.Cursor3D : null;
        private EditorCameraService CameraService => EditorCameraService.Instance;

        private bool _canMoveToPrefab = true;

        public void Set(PrefabConfiguration item, UnityAction<PrefabConfiguration> onClick, bool setSelectedOnClick = true, bool _isCameraStayingOnNavigatedPosition = false)
        {
            base.Set(item, onClick, setSelectedOnClick);

            isCameraStayingOnNavigatedPosition = _isCameraStayingOnNavigatedPosition;

            GameObject instancedPrefab = GameManager.Instance.MapBuilder
                .GetPrefabByGuid(item.SpawnPrefabOnBuild ? item.Guid : item.OwnerGuid);

            if (item.PrefabType is Enums.EPrefabType.Trigger) AddIcon(EIcon.Trigger);
            if (item.PrefabType is Enums.EPrefabType.TriggerReceiver) AddIcon(EIcon.TriggerReceiver);
            if (!item.SpawnPrefabOnBuild) AddIcon(EIcon.Embedded);

            if (instancedPrefab && instancedPrefab.GetBody()) AddIcon(EIcon.Wall);
            if (item is WallConfiguration configuration && configuration.HasPath()) AddIcon(EIcon.Move);

            Text.text = displayedItem.DisplayName;
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
            Cursor3D.ShowAt(displayedItem.TransformData.Position,
                UIManager.OpenedEditor.GetCursor3DScale(),
                displayedItem.TransformData.Rotation);

            _canMoveToPrefab = true;
            StartCoroutine(MouseOverCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor3D.Hide();
            _canMoveToPrefab = false;

            StopCoroutine(MouseOverCoroutine());

            if (_originalCameraTransformData != null)
            {
                MapEditorManager.Instance.SetFloor(_originalFloor);
                EditorCameraService.Instance.MoveCameraTo(_originalCameraTransformData);
            }

            _originalCameraTransformData = null;
        }

        private IEnumerator MouseOverCoroutine()
        {
            yield return _startNavigatingDelay;

            if (!_canMoveToPrefab) yield break;

            _canMoveToPrefab = true;
            _originalFloor = MapEditorManager.Instance.CurrentFloor;
            _originalCameraTransformData = CameraService.GetCameraTransformData();
            UIManager.OpenedEditor.MoveCameraToPrefab(Vector3Int.RoundToInt(displayedItem.TransformData.Position));
        }
    }
}