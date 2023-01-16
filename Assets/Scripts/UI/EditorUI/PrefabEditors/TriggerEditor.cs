﻿using System;
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
            Vector3 newPrefabLocalPosition = /*_prefabWallCenterPosition +*/ new Vector3(newPosition.z, newPosition.y, newPosition.x);
            Logger.Log($"New prefab local position: {newPrefabLocalPosition}.");
            EditedConfiguration.TransformData.Position = newPrefabLocalPosition;
            PhysicalPrefab.transform.position = newPrefabLocalPosition;
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
                // Quaternion inverseRotation = Quaternion.Inverse(PhysicalPrefab.transform.rotation);
                // Vector3 objectPositionInWallSpace = inverseRotation * PhysicalPrefab.transform.localPosition;
                // _prefabWallCenterPosition = new Vector3(objectPositionInWallSpace.x + 0.5f, objectPositionInWallSpace.y + 0.5f, objectPositionInWallSpace.z);
                // _prefabWallCenterPosition = PhysicalPrefab.transform.localPosition.ToVector3Int();
                // _prefabWallCenterPosition.x = (float) Math.Round(PhysicalPrefab.transform.localPosition.x, 1, MidpointRounding.ToEven);
                // Logger.Log($"WallCenter: {_prefabWallCenterPosition}, LocalPosition: {PhysicalPrefab.transform.localPosition}");

                _positionControl.ValueChanged.RemoveAllListeners();
                _positionControl.Label.text = $"{t.Get(Keys.Position)}:";
                Vector3 position = PhysicalPrefab.transform.position; //TODO continue here
                _positionControl.Value = PhysicalPrefab.transform.position /*- _prefabWallCenterPosition*/;
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