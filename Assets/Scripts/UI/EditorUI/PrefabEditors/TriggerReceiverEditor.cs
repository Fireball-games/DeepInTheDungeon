using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.Triggers;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerReceiverEditor : EditorWindowBase, IPrefabEditor
    {
        private readonly Vector3 _cursor3DScale = new(0.3f, 0.3f, 0.3f);

        private MapBuilder MapBuilder => GameManager.Instance.MapBuilder;

        private Transform _content;
        private Title _title;
        private Button _saveButton;
        private Button _cancelButton;
        private TMP_Text _statusText;
        private GameObject _editorWindow;
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
            InitializeComponents();
            VisualizeComponents();
            SetButtons();
            
            _title.SetActive(false);
            
            body.SetActive(true);
        }

        public void CloseWithRemovingChanges()
        {
            Logger.LogNotImplemented();
        }

        public void MoveCameraToPrefab(Vector3 worldPosition)
        {
            Logger.LogNotImplemented();
        }

        public Vector3 GetCursor3DScale() => _cursor3DScale;

        private void Save()
        {
            
        }

        private void Cancel()
        {
            
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
                _startPositionUpDown.Label.text = t.Get(Keys.StartPosition);
                _startPositionUpDown.maximum = positionsReceiver.steps.Count;
                _startPositionUpDown.Value = positionsReceiver.startPosition;
            }

            if (!anyComponentsShown)
            {
                SetStatusText(t.Get(Keys.NothingToEditForConfiguration));
            }
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
            _saveButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Save);
            _cancelButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Cancel);
        }

        private void AssignComponents()
        {
            Transform frame = body.transform.Find("Background/Frame");
            _content = frame.Find("Content");
            _title = frame.Find("Header/PrefabTitle").GetComponent<Title>();
            _saveButton = frame.Find("Buttons/SaveButton").GetComponent<Button>();
            _saveButton.onClick.AddListener(Save);
            _cancelButton = frame.Find("Buttons/CancelButton").GetComponent<Button>();
            _cancelButton.onClick.AddListener(Cancel);
            _statusText = _content.Find("StatusText").GetComponent<TMP_Text>();
            _editorWindow = body.transform.Find("Background").gameObject;
            _startPositionUpDownWrapper = _content.Find("StartPositionWrapper").gameObject;
            _startPositionUpDown = _startPositionUpDownWrapper.transform.Find("UpDown").GetComponent<NumericUpDown>();
            _existingReceivers = body.transform.Find("ExistingPrefabs").GetComponent<ConfigurationList>();
        }
    }
}