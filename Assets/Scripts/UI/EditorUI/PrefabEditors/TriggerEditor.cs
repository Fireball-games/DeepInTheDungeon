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
        private Vector3 _editedPrefabPosition;
        private int _editedPrefabRotationY;
        private readonly Vector3 _wallCursor3DSize = new(0.15f, 1.1f, 1.1f);
        private readonly Vector3 _genericCursor3DSize = new(0.33f, 0.33f, 0.33f);

        private Transform _content;

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
            TriggerType = Enums.ETriggerType.Repeat,
            Count = int.MaxValue,
            StartPosition = 1,
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
            Vector3 newPrefabWorldPosition = _prefabWallCenterPosition;
            switch (_editedPrefabRotationY)
            {
                case 0:
                    newPrefabWorldPosition += new Vector3(-newPosition.z, newPosition.y, -newPosition.x);
                    break;
                case 90:
                    newPrefabWorldPosition += new Vector3(-newPosition.x, newPosition.y, newPosition.z);
                    break;
                case 180:
                    newPrefabWorldPosition += new Vector3(newPosition.z, newPosition.y, newPosition.x);
                    break;
                case 270:
                    newPrefabWorldPosition += new Vector3(newPosition.x, newPosition.y, -newPosition.z);
                    break;
                default:
                    Logger.Log($"Wrong Y rotation detected: {_editedPrefabRotationY}");
                    break;
            }

            Logger.Log($"New prefab position: {newPrefabWorldPosition}");
            EditedConfiguration.TransformData.Position = newPrefabWorldPosition;
            PhysicalPrefab.transform.localPosition = newPrefabWorldPosition;
        }

        private void OnTriggerTypeChanged(int enumIntValue)
        {
            SetEdited();
            EditedConfiguration.TriggerType = enumIntValue.GetEnumValue<Enums.ETriggerType>();
            EditedConfiguration.Count = EditedConfiguration.TriggerType switch
            {
                Enums.ETriggerType.OneOff => 1,
                Enums.ETriggerType.Repeat => int.MaxValue,
                Enums.ETriggerType.Multiple => 3,
                _ => throw new ArgumentOutOfRangeException()
            };

            VisualizeOtherComponents();
        }

        private void OnTriggerCountChanged(float newCount)
        {
            SetEdited();
            EditedConfiguration.Count = (int) newCount;
        }

        private void OnReceiverListChanged(IEnumerable<PrefabConfiguration> updatedList)
        {
            SetEdited();
            EditedConfiguration.Subscribers = updatedList.Select(ExtractReceiverGuid).ToList();
            VisualizeOtherComponents();
        }

        protected override void InitializeOtherComponents()
        {
            _content = transform.Find("Body/Background/Frame/ScrollView/Viewport/Content");

            _positionControl = _content.Find("PositionControl").GetComponent<Vector3Control>();
            _positionControl.SetActive(false);

            _triggerTypeDropdown = _content.Find("TriggerTypeDropdown").GetComponent<LabeledDropdown>();
            _triggerTypeDropdown.SetActive(false);

            _triggerCountUpDown = _content.Find("TriggerCountUpDown").GetComponent<NumericUpDown>();
            _triggerCountUpDown.SetActive(false);
            _triggerCountUpDown.OnValueChanged.RemoveAllListeners();
            _triggerCountUpDown.OnValueChanged.AddListener(OnTriggerCountChanged);

            _receiverList = _content.Find("ReceiverList").GetComponent<EditableConfigurationList>();
            _receiverList.SetActive(false);
        }

        protected override void VisualizeOtherComponents()
        {
            _positionControl.Reparent(body.transform, false);
            _triggerTypeDropdown.Reparent(body.transform, false);
            _triggerCountUpDown.Reparent(body.transform, false);
            _receiverList.Reparent(body.transform, false);

            if (EditedConfiguration is null) return;

            if (EditedConfiguration.SpawnPrefabOnBuild)
            {
                Vector3 prefabPosition = PhysicalPrefab.transform.position;
                _editedPrefabRotationY = Mathf.RoundToInt(PhysicalPrefab.transform.rotation.eulerAngles.y);
                _prefabWallCenterPosition = prefabPosition.ToVector3Int();

                if (Mathf.RoundToInt(_editedPrefabRotationY) == 0)
                    _prefabWallCenterPosition.x = (float) Math.Round(PhysicalPrefab.transform.position.x, 1);
                if (Mathf.RoundToInt(_editedPrefabRotationY) == 90)
                    _prefabWallCenterPosition.z = (float) Math.Round(PhysicalPrefab.transform.position.z, 1);
                if (Mathf.RoundToInt(_editedPrefabRotationY) == 180)
                    _prefabWallCenterPosition.x = (float) Math.Round(PhysicalPrefab.transform.position.x, 1);
                if (Mathf.RoundToInt(_editedPrefabRotationY) == 270)
                    _prefabWallCenterPosition.z = (float) Math.Round(PhysicalPrefab.transform.position.z, 1);

                Logger.Log($"WallCenter: {_prefabWallCenterPosition}, prefab position: {prefabPosition}");

                _positionControl.ValueChanged.RemoveAllListeners();
                _positionControl.Label.text = $"{t.Get(Keys.Position)}:";
                _positionControl.Value = PhysicalPrefab.transform.position - _prefabWallCenterPosition;
                _positionControl.ValueChanged.AddListener(OnPositionChanged);
                _positionControl.Reparent(_content);
            }

            _triggerTypeDropdown.Set($"{t.Get(Keys.TriggerType)}:",
                EditedConfiguration.TriggerType,
                OnTriggerTypeChanged);
            _triggerTypeDropdown.Reparent(_content);

            if (EditedConfiguration.TriggerType is Enums.ETriggerType.Multiple)
            {
                _triggerCountUpDown.Value = EditedConfiguration.Count;
                _triggerCountUpDown.Label.text = $"{t.Get(Keys.Count)} :";
                _triggerCountUpDown.Reparent(_content);
            }

            IEnumerable<TriggerReceiverConfiguration> subscribers =
                EditedConfiguration.Subscribers.Select(s => MapBuilder.GetConfigurationByGuid<TriggerReceiverConfiguration>(s));

            _receiverList.Set(t.Get(Keys.SubscribedReceivers), subscribers, OnReceiverListChanged);
            _receiverList.Reparent(_content);
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