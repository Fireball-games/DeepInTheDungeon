using System.Collections;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.MapEditor.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationListButton : ListButtonBase<PrefabConfiguration>, IPointerEnterHandler, IPointerExitHandler
    {
        private readonly WaitForSecondsRealtime _startNavigatingDelay = new(0.5f);

        public override void Set(PrefabConfiguration item, UnityAction<PrefabConfiguration> onClick)
        {
            base.Set(item, onClick);

            if (item.PrefabType is Enums.EPrefabType.Trigger) AddIcon(IconStore.EIcon.Trigger);
            if (item.PrefabType is Enums.EPrefabType.TriggerReceiver) AddIcon(IconStore.EIcon.TriggerReceiver);
            if (!item.SpawnPrefabOnBuild) AddIcon(IconStore.EIcon.Embedded);
            
            if (item is WallConfiguration configuration && configuration.HasPath()) AddIcon(IconStore.EIcon.Move);
            
            Text.text = displayedItem.DisplayName;
        }

        protected override void OnClick_internal() => OnClick.Invoke(displayedItem);

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartCoroutine(MouseOverCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(MouseOverCoroutine());
        }

        private IEnumerator MouseOverCoroutine()
        {
            yield return _startNavigatingDelay;
            
            EditorCameraService.Instance.MoveCameraToPrefab(displayedItem.TransformData.Position);
        }
    }
}