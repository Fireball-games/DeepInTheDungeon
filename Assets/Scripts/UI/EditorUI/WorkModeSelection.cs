using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class WorkModeSelection : UIElementBase
    {
        [SerializeField] private ImageButton buildModeButton;
        [SerializeField] private BuildModeExpandedOptions buildModeOptions;
        [SerializeField] private ImageButton selectModeButton;
        [SerializeField] private ImageButton wallModeButton;
        [SerializeField] private ImageButton prefabTileModeButton;

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private Dictionary<ImageButton, EWorkMode> _workModesMap;

        private void Awake()
        {
            _workModesMap = new()
            {
                {buildModeButton, EWorkMode.Build},
                {selectModeButton, EWorkMode.Select},
                {wallModeButton, EWorkMode.Walls},
                {prefabTileModeButton, EWorkMode.PrefabTiles},
            };
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            buildModeButton.OnClickWithSender += WorkModeButtonClicked;
            buildModeButton.OnSelected += ActivateBuildModeOptions;
            buildModeButton.OnDeselected += DeactivateBuildModeOptions;
            selectModeButton.OnClickWithSender += WorkModeButtonClicked;
            wallModeButton.OnClickWithSender += WorkModeButtonClicked;
            prefabTileModeButton.OnClickWithSender += WorkModeButtonClicked;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            buildModeButton.OnClickWithSender -= WorkModeButtonClicked;
            buildModeButton.OnSelected -= ActivateBuildModeOptions;
            buildModeButton.OnDeselected -= DeactivateBuildModeOptions;
            wallModeButton.OnClickWithSender -= WorkModeButtonClicked;
            prefabTileModeButton.OnClickWithSender -= WorkModeButtonClicked;
        }

        private void ActivateBuildModeOptions()
        {
            buildModeOptions.SetActive(true);
            buildModeOptions.SetSelected(Manager.WorkLevel);
        }

        private void DeactivateBuildModeOptions()
        {
            buildModeOptions.SetActive(false);
        }

        private void OnWorkModeChanged(EWorkMode newWorkMode)
        {
            foreach (KeyValuePair<ImageButton, EWorkMode> record in _workModesMap)
            {
                if (record.Value == newWorkMode)
                {
                    record.Key.SetSelected(true);
                    continue;
                }
                
                record.Key.SetSelected(false);
            }
        }

        private void WorkModeButtonClicked(MonoBehaviour sender)
        {
            ImageButton button = sender as ImageButton;

            if (!button) return;
            
            Manager.SetWorkMode(_workModesMap[button]);
        }
    }
}