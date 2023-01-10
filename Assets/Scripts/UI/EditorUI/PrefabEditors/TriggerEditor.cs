using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.Triggers;
using Scripts.UI.Components;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerEditor : PrefabEditorBase<TriggerConfiguration, Trigger>
    {
        private Vector3Control _positionControl;

        private Vector3 _prefabWallCenterPosition;
        
        protected override Vector3 Cursor3DScale => new(0.15f, 1.1f, 1.1f);
        
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

        protected override IEnumerable<TriggerConfiguration> GetAvailableConfigurations()
        {
            return MapBuilder.MapDescription.PrefabConfigurations
                .Where(c => c.PrefabType == Enums.EPrefabType.Trigger)
                .Select(c => c as TriggerConfiguration);
        }

        private void OnPositionChanged(Vector3 newPosition)
        {
            Vector3 newPrefabWorldPosition = _prefabWallCenterPosition + newPosition;
            Logger.Log($"New prefab position: {newPrefabWorldPosition}");
            EditedConfiguration.TransformData.Position = newPrefabWorldPosition;
            PhysicalPrefab.transform.localPosition = newPrefabWorldPosition;
        }

        protected override void InitializeOtherComponents()
        {
            Transform frame = transform.Find("Body/Background/Frame");
            _positionControl = frame.Find("PositionControl").GetComponent<Vector3Control>();
            _positionControl.SetActive(false);
        }

        protected override void VisualizeOtherComponents()
        {
            if (EditedConfiguration is {SpawnPrefabOnBuild: false}) return;
            
            _prefabWallCenterPosition = PhysicalPrefab.transform.position.ToVector3Int();
            _prefabWallCenterPosition.x = (float)Math.Round(PhysicalPrefab.transform.position.x, 1);
            Logger.Log($"WallCenter: {_prefabWallCenterPosition}");
            
            _positionControl.OnValueChanged.RemoveAllListeners();
            _positionControl.Label.text = t.Get(Keys.Position);
            _positionControl.XMinimumMaximum = new Vector2(-0.5f, 0.5f);
            _positionControl.XMinimumMaximum = new Vector2(-0.5f, 0.5f);
            _positionControl.XMinimumMaximum = new Vector2(0f, 0.2f);
            _positionControl.Step = 0.01f;
            _positionControl.Value = PhysicalPrefab.transform.position - _prefabWallCenterPosition;
            _positionControl.OnValueChanged.AddListener(OnPositionChanged);
            _positionControl.SetActive(true);
        }
    }
}