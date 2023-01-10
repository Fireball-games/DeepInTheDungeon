using System.Collections;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
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
        private readonly WaitForSecondsRealtime _startNavigatingDelay = new(0.5f);
        private PositionRotation _originalCameraTransformData;

        private bool _canMoveToPrefab = true;

        public override void Set(PrefabConfiguration item, UnityAction<PrefabConfiguration> onClick)
        {
            base.Set(item, onClick);

            GameObject instancedPrefab = GameManager.Instance.MapBuilder.GetPrefabByGuid(item.Guid);
            
            if (instancedPrefab)
            {
                PrefabBase prefabScript = instancedPrefab.GetComponent<PrefabBase>();
            }

            if (item.PrefabType is Enums.EPrefabType.Trigger) AddIcon(EIcon.Trigger);
            if (item.PrefabType is Enums.EPrefabType.TriggerReceiver) AddIcon(EIcon.TriggerReceiver);
            if (!item.SpawnPrefabOnBuild) AddIcon(EIcon.Embedded);
            
            if (instancedPrefab && instancedPrefab.GetBody()) AddIcon(EIcon.Wall);
            if (item is WallConfiguration configuration && configuration.HasPath()) AddIcon(EIcon.Move);
            
            Text.text = displayedItem.DisplayName;
        }

        protected override void OnClick_internal() => OnClick.Invoke(displayedItem);

        public void OnPointerEnter(PointerEventData eventData)
        {
            _canMoveToPrefab = true;
            StartCoroutine(MouseOverCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _canMoveToPrefab = false;
            StopCoroutine(MouseOverCoroutine());
            
            if (_originalCameraTransformData != null)
            {
                EditorCameraService.Instance.MoveCameraTo(_originalCameraTransformData);
            }

            _originalCameraTransformData = null;
        }

        private IEnumerator MouseOverCoroutine()
        {
            yield return _startNavigatingDelay;

            if (!_canMoveToPrefab) yield break;

            _canMoveToPrefab = true;
            _originalCameraTransformData = EditorCameraService.Instance.GetCameraTransformData();
            EditorUIManager.Instance.MoveCameraToPrefab(displayedItem.TransformData.Position);
        }
    }
}