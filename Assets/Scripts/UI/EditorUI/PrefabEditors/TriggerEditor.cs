using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.Triggers;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerEditor : PrefabEditorBase<TriggerConfiguration, Trigger>
    {
        private Vector3Control _positionControl;
        private LabeledDropdown _triggerTypeDropdown;
        private NumericUpDown _triggerCountUpDown;
        private EditableConfigurationList _receiverList;

        private Vector3 _prefabWallCenterPosition;
        private readonly Vector3 _wallCursor3DSize = new(0.15f, 1.1f, 1.1f);
        private readonly Vector3 _genericCursor3DSize = new(0.33f, 0.33f, 0.33f);

        protected override void RemoveOtherComponents()
        {
            
        }

        public override Vector3 GetCursor3DScale() =>
            EditedConfiguration == null
                ? IsPrefabFinderActive ? _genericCursor3DSize : _wallCursor3DSize
                : _genericCursor3DSize;

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
            SetEdited();
            Vector3 newPrefabWorldPosition = _prefabWallCenterPosition + new Vector3(newPosition.z, newPosition.x, newPosition.y);
            Logger.Log($"New prefab position: {newPrefabWorldPosition}");
            EditedConfiguration.TransformData.Position = newPrefabWorldPosition;
            PhysicalPrefab.transform.localPosition = newPrefabWorldPosition;
        }

        private void OnTriggerTypeChanged(int enumIntValue)
        {
            SetEdited();
            EditedConfiguration.TriggerType = enumIntValue.GetEnumValue<Enums.ETriggerType>();
            VisualizeOtherComponents();
        }
        
        private void OnReceiverListChanged(IEnumerable<PrefabConfiguration> updatedList)
        {
            SetEdited();
            EditedConfiguration.Subscribers = updatedList.Select(ExtractReceiverGuid).ToList();
            VisualizeOtherComponents();
        }

        protected override void InitializeOtherComponents()
        {
            Transform content = transform.Find("Body/Background/Frame/ScrollView/Viewport/Content");
            
            _positionControl = content.Find("PositionControl").GetComponent<Vector3Control>();
            _positionControl.SetActive(false);
            
            _triggerTypeDropdown = content.Find("TriggerTypeDropdown").GetComponent<LabeledDropdown>();
            _triggerTypeDropdown.SetActive(false);

            _triggerCountUpDown = content.Find("TriggerCountUpDown").GetComponent<NumericUpDown>();
            _triggerCountUpDown.SetActive(false);

            _receiverList = content.Find("ReceiverList").GetComponent<EditableConfigurationList>();
            _receiverList.SetActive(false);
        }

        protected override void VisualizeOtherComponents()
        {
            _positionControl.SetActive(false);
            _triggerTypeDropdown.SetActive(false);
            _triggerCountUpDown.SetActive(false);

            if (EditedConfiguration is null) return;

            _triggerTypeDropdown.Set( $"{t.Get(Keys.TriggerType)}:",
                EditedConfiguration.TriggerType,
                OnTriggerTypeChanged);
            _triggerTypeDropdown.SetActive(true);

            IEnumerable<TriggerReceiverConfiguration> subscribers =
                EditedConfiguration.Subscribers.Select(s => MapBuilder.GetConfigurationByGuid<TriggerReceiverConfiguration>(s));

            _receiverList.Set(t.Get(Keys.SubscribedReceivers), subscribers, OnReceiverListChanged);
            _receiverList.SetActive(true);
            
            if (EditedConfiguration.TriggerType is Enums.ETriggerType.XTimes)
            {
                _triggerCountUpDown.Value = EditedConfiguration.Count;
                _triggerCountUpDown.Label.text = $"{t.Get(Keys.Count)} :";
                _triggerCountUpDown.SetActive(true);
            }

            if (!EditedConfiguration.SpawnPrefabOnBuild) return;

            _prefabWallCenterPosition = PhysicalPrefab.transform.position.ToVector3Int();
            _prefabWallCenterPosition.x = (float) Math.Round(PhysicalPrefab.transform.position.x, 1);
            Logger.Log($"WallCenter: {_prefabWallCenterPosition}");

            _positionControl.ValueChanged.RemoveAllListeners();
            _positionControl.Label.text = $"{t.Get(Keys.Position)}:";
            _positionControl.Value = PhysicalPrefab.transform.position - _prefabWallCenterPosition;
            _positionControl.ValueChanged.AddListener(OnPositionChanged);
            _positionControl.SetActive(true);
        }

        private string ExtractReceiverGuid(PrefabConfiguration configuration)
        {
            if (configuration is TriggerReceiverConfiguration receiver)
            {
                return receiver.Guid;
            }
            
            Logger.Log($"Attempt to work some other object, where {nameof(TriggerReceiverConfiguration)} is expected.");
            return null;
        }
    }
}