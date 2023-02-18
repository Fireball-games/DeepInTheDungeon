using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.Triggers;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using UnityEngine;
using static Scripts.Enums;
using static Scripts.MapEditor.Services.PathsService;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerEditor : PrefabEditorBase<TriggerConfiguration, Trigger, TriggerService>
    {
        private Vector3Control _positionControl;
        private NumericUpDown _startPositionUpDown;
        private LabeledDropdown _triggerTypeDropdown;
        private NumericUpDown _triggerCountUpDown;

        private LabeledDropdown _targetMapDropdown;
        private LabeledDropdown _entryPointDropdown;
        private NumericUpDown _exitDelayUpDown;

        private EditableConfigurationList _receiverList;

        private Vector3 _prefabWallCenterPosition;
        private Vector3 _editedPrefabPosition;
        private int _editedPrefabRotationY;
        private readonly Vector3 _wallCursor3DSize = new(0.15f, 1.1f, 1.1f);
        private readonly Vector3 _genericCursor3DSize = new(0.33f, 0.33f, 0.33f);
        private readonly Vector3 _tileTriggerScale = new(0.75f, 0.75f, 0.75f);

        protected override void RemoveOtherComponents()
        {
            if (EditedConfiguration != null)
            {
                DestroyPath(EPathsType.Trigger, EditedConfiguration.Guid);
            }
        }

        public override Vector3 GetCursor3DScale()
        {
            bool isWallTrigger = EditedPrefabType == EPrefabType.TriggerOnWall;

            if (EditedConfiguration != null) return isWallTrigger 
                ? _wallCursor3DSize 
                : Vector3.one;
            
            if (IsPrefabFinderActive) return _genericCursor3DSize;
            
            return isWallTrigger 
                ? _wallCursor3DSize 
                : Vector3.one;

        }

        protected override Vector3 GetPlaceholderScale() => EditedPrefabType == EPrefabType.TriggerOnWall
            ? DefaultPlaceholderScale
            : _tileTriggerScale;

        protected override TriggerConfiguration GetNewConfiguration(string prefabName)
        {
            Trigger storePrefab = PrefabStore.GetPrefabByName<Trigger>(prefabName);
            return new()
            {
                Guid = Guid.NewGuid().ToString(),
                PrefabType = EditedPrefabType,
                PrefabName = prefabName,
                TransformData = new PositionRotation(SelectedCage.transform.position, SelectedCage.transform.rotation),
                SpawnPrefabOnBuild = true,

                Subscribers = new List<string>(),
                TriggerType = storePrefab.triggerType,
                Count = storePrefab.triggerType == ETriggerType.OneOff
                    ? 1
                    : storePrefab.triggerType == ETriggerType.Repeat
                        ? 2
                        : int.MaxValue,
                StartPosition = 1,
            };
        }

        protected override TriggerConfiguration CloneConfiguration(TriggerConfiguration sourceConfiguration) => new(sourceConfiguration);

        protected override void RemoveAndReopen()
        {
            if (IsCurrentConfigurationChanged)
            {
                DestroyPath(EPathsType.Trigger, EditedConfiguration.Guid);

                RemoveOtherComponents();
            }
            else if (EditedConfiguration != null)
            {
                HighlightPath(EPathsType.Trigger, EditedConfiguration.Guid, false);
            }

            base.RemoveAndReopen();
        }

        protected override void SaveMap()
        {
            HighlightPath(EPathsType.Trigger, EditedConfiguration.Guid, false);

            base.SaveMap();
        }

        private void MoveCameraToPrefabFocused(PositionRotation targetData)
        {
            Vector3 localForward = targetData.Rotation * Vector3.forward;
            targetData.Position += localForward;
            targetData.Rotation = Quaternion.LookRotation(localForward, targetData.Rotation * Vector3.up);
            EditorCameraService.Instance.MoveCameraTo(targetData);
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

            // Logger.Log($"New prefab position: {newPrefabWorldPosition}");
            EditedConfiguration.TransformData.Position = newPrefabWorldPosition;
            PhysicalPrefab.transform.localPosition = newPrefabWorldPosition;

            RedrawPath();
        }

        private void OnTriggerTypeChanged(int enumIntValue)
        {
            SetEdited();
            EditedConfiguration.TriggerType = enumIntValue.GetEnumValue<ETriggerType>();
            EditedConfiguration.Count = EditedConfiguration.TriggerType switch
            {
                ETriggerType.OneOff => 1,
                ETriggerType.Repeat => int.MaxValue,
                ETriggerType.Multiple => 3,
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
            RedrawPath();
            VisualizeOtherComponents();
        }

        private void OnStartPositionChanged(float newPosition)
        {
            SetEdited();
            int position = (int) newPosition;
            EditedConfiguration.StartPosition = position;
            IPositionsTrigger positionTrigger = EditedPrefab as IPositionsTrigger;
            positionTrigger!.SetStartPosition(position);
            positionTrigger!.SetPosition();
        }

        private void OnTargetMapChanged(string targetMap)
        {
            SetEdited();
            EditedConfiguration.TargetMapName = targetMap;
            VisualizeOtherComponents();
        }

        private void OnEntryPointChanged(string entryPointName)
        {
            SetEdited();
            EditedConfiguration.TargetMapEntranceName = entryPointName;
        }

        private void OnExitDelayChanged(float exitDelay)
        {
            SetEdited();
            EditedConfiguration.ExitDelay = exitDelay;
        }

        private void RedrawPath()
        {
            AddReplaceTriggerPath(EditedConfiguration, true);
        }

        protected override void InitializeOtherComponents()
        {
            _positionControl = Content.Find("PositionControl").GetComponent<Vector3Control>();

            _startPositionUpDown = Content.Find("StartPositionUpDown").GetComponent<NumericUpDown>();
            _startPositionUpDown.OnValueChanged.AddListener(OnStartPositionChanged);

            _triggerTypeDropdown = Content.Find("TriggerTypeDropdown").GetComponent<LabeledDropdown>();

            _triggerCountUpDown = Content.Find("TriggerCountUpDown").GetComponent<NumericUpDown>();
            _triggerCountUpDown.OnValueChanged.RemoveAllListeners();
            _triggerCountUpDown.OnValueChanged.AddListener(OnTriggerCountChanged);

            _receiverList = Content.Find("ReceiverList").GetComponent<EditableConfigurationList>();

            _targetMapDropdown = Content.Find("TargetMapDropdown").GetComponent<LabeledDropdown>();
            _entryPointDropdown = Content.Find("EntryPointDropdown").GetComponent<LabeledDropdown>();
            _exitDelayUpDown = Content.Find("ExitDelayUpDown").GetComponent<NumericUpDown>();
        }

        protected override void VisualizeOtherComponents()
        {
            _positionControl.SetCollapsed(true);
            _startPositionUpDown.SetCollapsed(true);
            _triggerTypeDropdown.SetCollapsed(true);
            _triggerCountUpDown.SetCollapsed(true);
            _receiverList.SetCollapsed(true);
            _targetMapDropdown.SetCollapsed(true);
            _entryPointDropdown.SetCollapsed(true);
            _exitDelayUpDown.SetCollapsed(true);

            if (EditedConfiguration is null) return;
            
            bool isExitPointTrigger = EditedPrefab is ExitPointTrigger;

            if (EditedConfiguration.SpawnPrefabOnBuild && !isExitPointTrigger)
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

                // Logger.Log($"WallCenter: {_prefabWallCenterPosition}, prefab position: {prefabPosition}");

                _positionControl.ValueChanged.RemoveAllListeners();
                _positionControl.Label.text = $"{t.Get(Keys.Position)}:";
                _positionControl.Value = prefabPosition - _prefabWallCenterPosition;
                _positionControl.ValueChanged.AddListener(OnPositionChanged);
                _positionControl.SetCollapsed(false);
            }

            if (!isExitPointTrigger && EditedPrefab is IPositionsTrigger positionsTrigger)
            {
                _startPositionUpDown.Label.text = t.Get(Keys.StartPosition);
                _startPositionUpDown.maximum = positionsTrigger.GetSteps().Count - 1;
                _startPositionUpDown.Value = positionsTrigger.GetStartPosition();
                _startPositionUpDown.SetCollapsed(false);
            }

            if (isExitPointTrigger)
            {
                Campaign currentCampaign = GameManager.CurrentCampaign;
                List<string> mapsNames = currentCampaign.MapsNames.ToList();

                if (string.IsNullOrEmpty(EditedConfiguration.TargetMapName))
                {
                    SetEdited();
                    EditedConfiguration.TargetMapName = mapsNames.FirstOrDefault();
                }

                _targetMapDropdown.Set($"{t.Get(Keys.TargetMap)}:",
                    mapsNames,
                    mapsNames.IndexOf(EditedConfiguration.TargetMapName),
                    OnTargetMapChanged);
                _targetMapDropdown.SetCollapsed(false);

                MapDescription targetMap = currentCampaign.GetMapByName(EditedConfiguration.TargetMapName);

                if (string.IsNullOrEmpty(EditedConfiguration.TargetMapEntranceName) ||
                    EditedConfiguration.TargetMapEntranceName == t.Get(Keys.NoNameSet))
                    OnEntryPointChanged(targetMap?.EntryPointsNames.FirstOrDefault());

                _entryPointDropdown.Set($"{t.Get(Keys.EntryPoint)}:",
                    currentCampaign.GetMapByName(EditedConfiguration.TargetMapName)?.EntryPointsNames ?? new List<string>(),
                    targetMap == null ? 0 : targetMap.EntryPointsNames.ToList().IndexOf(EditedConfiguration.TargetMapEntranceName),
                    OnEntryPointChanged);
                _entryPointDropdown.SetCollapsed(false);

                _exitDelayUpDown.Label.text = $"{t.Get(Keys.ExitDelay)}:";
                _exitDelayUpDown.OnValueChanged.RemoveAllListeners();
                _exitDelayUpDown.Value = EditedConfiguration.ExitDelay;
                _exitDelayUpDown.OnValueChanged.AddListener(OnExitDelayChanged);
                _exitDelayUpDown.SetCollapsed(false);
            }

            if (!isExitPointTrigger)
            {
                _triggerTypeDropdown.Set($"{t.Get(Keys.TriggerType)}:",
                    EditedConfiguration.TriggerType,
                    OnTriggerTypeChanged);
                _triggerTypeDropdown.SetCollapsed(false);

                if (EditedConfiguration.TriggerType is ETriggerType.Multiple)
                {
                    _triggerCountUpDown.Value = EditedConfiguration.Count;
                    _triggerCountUpDown.Label.text = $"{t.Get(Keys.Count)} :";
                    _triggerCountUpDown.SetCollapsed(false);
                }

                IEnumerable<TriggerReceiverConfiguration> subscribers =
                    EditedConfiguration.Subscribers.Select(s => MapBuilder.GetConfigurationByGuid<TriggerReceiverConfiguration>(s));

                _receiverList.Set(t.Get(Keys.SubscribedReceivers), subscribers, OnReceiverListChanged);
                _receiverList.SetCollapsed(false);
                RedrawPath();
            }
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