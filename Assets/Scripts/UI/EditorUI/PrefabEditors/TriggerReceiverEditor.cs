using System.Collections.Generic;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.Triggers;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerReceiverEditor : EditorWindowBase, IPrefabEditor
    {
        private readonly Vector3 _cursor3DScale = new(0.3f, 0.3f, 0.3f);

        private MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private CageController SelectedCursor => EditorUIManager.Instance.SelectedCage;

        private Transform _content;
        private Title _title;
        private Button _saveButton;
        private Button _cancelButton;
        private TMP_Text _statusText;
        private GameObject _startPositionUpDownWrapper;
        private NumericUpDown _startPositionUpDown;
        private ConfigurationList _existingReceivers;

        private TriggerReceiverConfiguration _originalConfiguration;
        private TriggerReceiverConfiguration _editedConfiguration;
        private TriggerReceiver _editedPrefab;

        private bool _isConfigurationEdited;

        private void Awake()
        {
            AssignComponents();
        }

        public void Open()
        {
            IEnumerable<TriggerReceiverConfiguration> availableConfigurations =
                MapBuilder.GetConfigurations<TriggerReceiverConfiguration>(Enums.EPrefabType.TriggerReceiver);
            
            _existingReceivers.Open(t.Get(Keys.AvailablePrefabs), availableConfigurations, SetEditedConfiguration);

            InitializeComponents();
            VisualizeComponents();
            SetButtons();

            _title.SetActive(false);

            body.SetActive(true);
        }

        public void CloseWithRemovingChanges()
        {
            RemoveChanges();
            Close();
        }

        public void MoveCameraToPrefab(Vector3 worldPosition) =>
            EditorCameraService.Instance.MoveCameraToPrefab(Vector3Int.RoundToInt(worldPosition));

        public Vector3 GetCursor3DScale() => _cursor3DScale;

        private void Save()
        {
            MapBuilder.AddReplacePrefabConfiguration(_editedConfiguration);
            MapEditorManager.Instance.SaveMap();
            SetEdited(false);
        }

        private void RemoveChanges()
        {
            if (_isConfigurationEdited)
            {
                MapBuilder.AddReplacePrefabConfiguration(_originalConfiguration);
                _editedConfiguration = _originalConfiguration;
                SetEdited(false);
            }
            
            SelectedCursor.Hide();
            VisualizeComponents();
        }

        private void Close()
        {
            _originalConfiguration = null;
            _editedConfiguration = null;

            SetActive(false);
        }

        private void SetEditedConfiguration(PrefabConfiguration clickedConfiguration)
        {
            if (clickedConfiguration is not TriggerReceiverConfiguration configuration) return;
            
            _originalConfiguration = configuration;
            _editedConfiguration = configuration;
            GameObject owner = MapBuilder.GetPrefabByGuid(configuration.OwnerGuid);
            _editedPrefab = owner.GetComponent<TriggerReceiver>();
            Vector3 ownerPosition = owner.transform.position;
            SelectedCursor.ShowAt(ownerPosition, GetCursor3DScale(), owner.transform.localRotation);
            SetStatusText();
            _title.Show(configuration.Identification);
            MoveCameraToPrefab(ownerPosition);
                
            VisualizeComponents();
        }

        private void SetEdited(bool isEdited)
        {
            _isConfigurationEdited = isEdited;
            SetButtons();
            EditorEvents.TriggerOnMapEditedStatusChanged(isEdited);
        }

        private void VisualizeComponents()
        {
            _startPositionUpDownWrapper.SetActive(false);

            if (_originalConfiguration == null)
            {
                SetStatusText(t.Get(Keys.SelectConfiguration));
                return;
            }

            bool anyComponentsShown = false;

            if (_editedPrefab is TriggerReceiverWithPositions positionsReceiver)
            {
                anyComponentsShown = true;
                _startPositionUpDownWrapper.SetActive(true);
                _startPositionUpDown.OnValueChanged.RemoveAllListeners();
                _startPositionUpDown.Label.text = t.Get(Keys.StartPosition);
                _startPositionUpDown.maximum = positionsReceiver.steps.Count - 1;
                _startPositionUpDown.Value = positionsReceiver.startPosition;
                _startPositionUpDown.OnValueChanged.AddListener(OnStartPositionChanged);
            }

            if (!anyComponentsShown)
            {
                SetStatusText(t.Get(Keys.NothingToEditForConfiguration));
            }
        }

        private void OnStartPositionChanged(float newValue)
        {
            SetEdited(true);
            int newPosition = (int) newValue;
            _editedConfiguration.StartPosition = newPosition;
            _editedPrefab.startPosition = newPosition;
            _editedPrefab.SetPosition();
        }

        private void SetButtons()
        {
            _saveButton.gameObject.SetActive(_isConfigurationEdited);
            _cancelButton.gameObject.SetActive(_isConfigurationEdited);
        }

        private void SetStatusText(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                _statusText.gameObject.SetActive(false);
                _statusText.text = "";
                return;
            }

            _statusText.gameObject.SetActive(true);
            _statusText.text = text;
        }

        private void InitializeComponents()
        {
            _saveButton.SetText(t.Get(Keys.Save));
            _saveButton.SetTextColor(Colors.Positive);
            _cancelButton.SetText(t.Get(Keys.Cancel));
            _cancelButton.SetTextColor(Colors.Negative);
        }

        private void AssignComponents()
        {
            Transform frame = body.transform.Find("Background/Frame");
            _content = frame.Find("Content");
            _title = frame.Find("Header/PrefabTitle").GetComponent<Title>();
            _saveButton = frame.Find("Buttons/SaveButton").GetComponent<Button>();
            _saveButton.onClick.AddListener(Save);
            _cancelButton = frame.Find("Buttons/CancelButton").GetComponent<Button>();
            _cancelButton.onClick.AddListener(RemoveChanges);
            _statusText = _content.Find("StatusText").GetComponent<TMP_Text>();
            _startPositionUpDownWrapper = _content.Find("StartPositionWrapper").gameObject;
            _startPositionUpDown = _startPositionUpDownWrapper.transform.Find("UpDown").GetComponent<NumericUpDown>();
            _existingReceivers = body.transform.Find("ExistingPrefabs").GetComponent<ConfigurationList>();
        }
    }
}