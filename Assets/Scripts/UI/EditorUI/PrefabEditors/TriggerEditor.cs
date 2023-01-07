using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Localization;
using Scripts.System;
using Scripts.Triggers;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerEditor : PrefabEditorBase<TriggerConfiguration, Trigger>
    {
        private Vector3Control _positionControl;

        private Vector3 _prefabWallCenterPosition;
        
        protected override TriggerConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
            SpawnPrefabOnBuild = true,
            
            Subscribers = new List<string>(),
        };

        protected override TriggerConfiguration CloneConfiguration(TriggerConfiguration sourceConfiguration) => new(sourceConfiguration);
        
        protected override Vector3 Cursor3DScale => new(0.15f, 1.1f, 1.1f);

        public override void Open(TriggerConfiguration configuration)
        {
            if (!CanOpen) return;

            if (configuration == null)
            {
                Close();
                return;
            }

            base.Open(configuration);

            VisualizeOtherComponents();
        }

        protected override string SetupWindow(Enums.EPrefabType prefabType, bool deleteButtonActive)
        {
            Initialize();

            return base.SetupWindow(prefabType, deleteButtonActive);
        }

        private void Initialize()
        {
            Transform frame = transform.Find("Body/Background/Frame");
            _positionControl = frame.Find("PositionControl").GetComponent<Vector3Control>();
            _positionControl.SetActive(false);
        }

        private void OnPositionChanged(Vector3 newPosition)
        {
            EditedConfiguration.TransformData.Position = newPosition;
            PhysicalPrefab.transform.localPosition += newPosition;
        }

        private void VisualizeOtherComponents()
        {
            _positionControl.OnValueChanged.RemoveAllListeners();
            _positionControl.Label.text = t.Get(Keys.Position);
            _positionControl.XMinimumMaximum = new Vector2(-0.5f, 0.5f);
            _positionControl.XMinimumMaximum = new Vector2(-0.5f, 0.5f);
            _positionControl.XMinimumMaximum = new Vector2(0f, 0.2f);
            _positionControl.Step = 0.01f;
            _positionControl.OnValueChanged.AddListener(OnPositionChanged);
            _positionControl.SetActive(true);
        }
    }
}