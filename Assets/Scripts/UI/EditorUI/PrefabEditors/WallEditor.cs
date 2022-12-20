using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.System;
using Scripts.UI.EditorUI.PrefabEditors;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.UI.EditorUI
{
    public class WallEditor : PrefabEditorBase<WallConfiguration, WallPrefabBase>
    {
        [SerializeField] private LabeledSlider offsetSlider;

        protected override WallConfiguration GetNewConfiguration(string prefabName)
        {
            return new WallConfiguration
            {
                PrefabType = EditedPrefabType,
                PrefabName = AvailablePrefabs.FirstOrDefault(prefab => prefab.name == prefabName)?.name,
                TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
                WayPoints = new List<Vector3>(),
                Offset = 0f
            };
        }

        protected override WallConfiguration CopyConfiguration(WallConfiguration sourceConfiguration) => new(EditedConfiguration);

        protected override Vector3 Cursor3DScale => new(0.15f, 1f, 1f);

        public override void Open(WallConfiguration configuration)
        {
            if (!CanOpen) return;
            
            if (configuration == null)
            {
                Close();
                return;
            }
            
            base.Open(configuration);

            PhysicalPrefab = MapBuilder.GetWallByConfiguration(configuration).GetComponentInChildren<MeshFilter>()?.gameObject;

            if (!PhysicalPrefab) return;
            
            offsetSlider.SetActive(true);
            offsetSlider.Value = configuration.Offset;
            offsetSlider.slider.onValueChanged.RemoveAllListeners();
            offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
        }

        protected override string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            offsetSlider.SetLabel(T.Get(Keys.Offset));
            offsetSlider.SetActive(false);
            
            return base.SetupWindow(prefabType, deleteButtonActive);
        }

        protected override void SetPrefab(string prefabName)
        {
            base.SetPrefab(prefabName);
            
            if (PhysicalPrefab)
            {
                offsetSlider.SetActive(true);
                offsetSlider.Value = EditedConfiguration.Offset;
                offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
            }
        }

        private void OnOffsetSliderValueChanged(float value)
        {
            ConfirmButton.gameObject.SetActive(true);
            EditorEvents.TriggerOnMapEdited();
            Vector3 newPosition = PhysicalPrefab.transform.localPosition;
            newPosition.x = value;
            EditedConfiguration.Offset = value;
            PhysicalPrefab.transform.localPosition = newPosition;
        }
    }
}