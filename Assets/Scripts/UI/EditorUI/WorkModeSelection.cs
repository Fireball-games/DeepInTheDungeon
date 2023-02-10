using System.Collections.Generic;
using System.Linq;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class WorkModeSelection : UIElementBase
    {
        private ImageButton _buildModeButton;
        private BuildModeExpandedOptions _buildModeOptions;
        private ImageButton _selectModeButton;
        private ExtendedOptionsBase _selectModeOptions;
        private ImageButton _wallModeButton;
        private ImageButton _prefabTileModeButton;
        private ImageButton _triggerModeButton;
        private ExtendedOptionsBase _triggerModeOptions;

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private Dictionary<ImageButton, EWorkMode[]> _workModesMap;

        private void Awake()
        {
            Transform content = body.transform.Find("Content");
            _buildModeButton = content.Find("BuildModeButton").GetComponent<ImageButton>();
            _buildModeOptions = _buildModeButton.transform.Find("BuildModeExpandedOptions").GetComponent<BuildModeExpandedOptions>();
            _selectModeButton = content.Find("SelectModeButton").GetComponent<ImageButton>();
            _selectModeOptions = _selectModeButton.transform.Find("SelectModeExpandedOptions").GetComponent<ExtendedOptionsBase>();
            _wallModeButton = content.Find("WallModeButton").GetComponent<ImageButton>();
            _prefabTileModeButton = content.Find("PrefabTileModeButton").GetComponent<ImageButton>();
            _triggerModeButton = content.Find("TriggerModeButton").GetComponent<ImageButton>();
            _triggerModeOptions = _triggerModeButton.transform.Find("TriggerModeExpandedOptions").GetComponent<ExtendedOptionsBase>();
            
            _workModesMap = new()
            {
                {_buildModeButton, new[] { EWorkMode.Build }},
                {_selectModeButton, new[] { EWorkMode.SetWalls, EWorkMode.EditEntryPoints }},
                {_wallModeButton, new[] { EWorkMode.Walls }},
                {_prefabTileModeButton, new[] { EWorkMode.PrefabTiles}},
                {_triggerModeButton, new[] { EWorkMode.Triggers, EWorkMode.TriggerReceivers}},
            };
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnPrefabEdited += OnPrefabEdited;
            _buildModeButton.OnClickWithSender += WorkModeButtonClicked;
            _buildModeButton.OnSelected += ActivateBuildModeOptions;
            _buildModeButton.OnDeselected += DeactivateBuildModeOptions;
            _selectModeButton.OnClickWithSender += WorkModeButtonClicked;
            _wallModeButton.OnClickWithSender += WorkModeButtonClicked;
            _prefabTileModeButton.OnClickWithSender += WorkModeButtonClicked;
            _triggerModeButton.OnClickWithSender += TriggerWorkModeClicked;
            _triggerModeButton.OnSelected += ActivateTriggerModeOptions;
            _triggerModeButton.OnDeselected += DeactivateTriggerModeOptions;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnPrefabEdited -= OnPrefabEdited;
            _buildModeButton.OnClickWithSender -= WorkModeButtonClicked;
            _buildModeButton.OnSelected -= ActivateBuildModeOptions;
            _buildModeButton.OnDeselected -= DeactivateBuildModeOptions;
            _wallModeButton.OnClickWithSender -= WorkModeButtonClicked;
            _prefabTileModeButton.OnClickWithSender -= WorkModeButtonClicked;
            _triggerModeButton.OnClickWithSender -= TriggerWorkModeClicked;
            _triggerModeButton.OnSelected -= ActivateTriggerModeOptions;
            _triggerModeButton.OnDeselected -= DeactivateTriggerModeOptions;
        }

        private void ActivateBuildModeOptions()
        {
            _buildModeOptions.SetActive(true);
            _buildModeOptions.SetSelected(Manager.WorkLevel);
        }
        
        private void ActivateTriggerModeOptions()
        {
            _triggerModeOptions.SetActive(true);
            _triggerModeOptions.SetSelected(Manager.WorkMode);
        }

        private void DeactivateBuildModeOptions() => _buildModeOptions.SetActive(false);
        private void DeactivateTriggerModeOptions() => _triggerModeOptions.SetActive(false);

        private void OnWorkModeChanged(EWorkMode newWorkMode)
        {
            foreach (KeyValuePair<ImageButton, EWorkMode[]> record in _workModesMap)
            {
                if (record.Value.Contains(newWorkMode))
                {
                    record.Key.SetSelected(true);

                    // if (newWorkMode is EWorkMode.Triggers or EWorkMode.TriggerReceivers)
                    if (_workModesMap[_triggerModeButton].Contains(newWorkMode))
                    {
                        _triggerModeOptions.SetActive(true);
                        _triggerModeOptions.SetSelected(newWorkMode);
                    }
                    
                    if (_workModesMap[_selectModeButton].Contains(newWorkMode))
                    {
                        _selectModeOptions.SetActive(true);
                        _selectModeOptions.SetSelected(newWorkMode);
                    }
                    
                    continue;
                }
                
                record.Key.SetSelected(false);
            }
        }

        private void OnPrefabEdited(bool isEdited)
        {
            _workModesMap.Keys.ForEach(button => button.SetInteractable(!isEdited));
        }

        private void WorkModeButtonClicked(MonoBehaviour sender)
        {
            ImageButton button = sender as ImageButton;

            if (!button) return;

            int modeIndex = button == _triggerModeButton
                ? _triggerModeOptions.LastSelectedMode == EWorkMode.Triggers ? 0 : 1
                : 0;
            
            Manager.SetWorkMode(_workModesMap[button][modeIndex]);
        }

        private void TriggerWorkModeClicked(MonoBehaviour sender)
        {
            ImageButton button = sender as ImageButton;

            if (!button) return;
            
            Manager.SetWorkMode(_triggerModeOptions.LastSelectedMode);
        }
    }
}