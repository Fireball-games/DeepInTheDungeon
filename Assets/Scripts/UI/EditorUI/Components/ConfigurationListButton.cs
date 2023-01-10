using System.Collections;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.MapEditor.Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationListButton : ListButtonBase<PrefabConfiguration>, IPointerEnterHandler, IPointerExitHandler
    {
        private readonly WaitForSecondsRealtime _startNavigatingDelay = new(0.5f);
        
        protected override void SetItemName()
        {
            if(!Text) Initialize();
            
            if (displayedItem.PrefabType is Enums.EPrefabType.Trigger) AddIcon(IconStore.EIcon.Trigger);
            if (displayedItem.PrefabType is Enums.EPrefabType.TriggerReceiver) AddIcon(IconStore.EIcon.TriggerReceiver);
            if (!displayedItem.SpawnPrefabOnBuild) AddIcon(IconStore.EIcon.Embedded);
            
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