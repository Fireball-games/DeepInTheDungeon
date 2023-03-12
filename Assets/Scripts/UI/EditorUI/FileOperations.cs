using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI
{
    public class FileOperations : UIElementBase
    {
        private Button _exitButton;
        private Button _saveButton;
        private Button _addOpenMapButton;

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static EditorUIManager EditorUIManager => EditorUIManager.Instance;

        private void Awake()
        {
            Transform bodyTransform = body.transform;
            
            _saveButton = bodyTransform.Find("SaveButton").GetComponent<Button>();
            _saveButton.SetTextColor(Colors.Positive);
            _saveButton.onClick.AddListener(OnSaveClicked);
            
            _exitButton = bodyTransform.Find("ExitButton").GetComponent<Button>();
            _exitButton.onClick.AddListener(OnExitClicked);
            
            _addOpenMapButton = bodyTransform.Find("AddOpenMapButton").GetComponent<Button>();            
            _addOpenMapButton.onClick.AddListener(OnNewMapClicked);
        }

        private void OnEnable()
        {
            EditorEvents.OnPrefabEdited += OnMapChanged;
            EditorEvents.OnMapLayoutChanged += OnMapChanged;
            EditorEvents.OnMapSaved += OnMapChanged;
            
            RedrawButtons();
        }

        private void OnDisable()
        {
            EditorEvents.OnPrefabEdited -= OnMapChanged;
            EditorEvents.OnMapLayoutChanged -= OnMapChanged;
            EditorEvents.OnMapSaved -= OnMapChanged;
        }

        private void OnMapChanged(bool _) => OnMapChanged();

        private void OnMapChanged() => RedrawButtons();

        private void RedrawButtons()
        {
            _exitButton.SetText(t.Get(Keys.Exit));
            _saveButton.SetText(t.Get(Keys.Save));
            _addOpenMapButton.SetText(t.Get(Keys.AddOpenMap));

            if (Manager)
            {
                _saveButton.gameObject.SetActive(Manager.MapIsChanged || Manager.PrefabIsEdited);
            }
        }

        private void OnSaveClicked()
        {
            Manager.SaveMap();
            RedrawButtons();
        }

        private async void OnNewMapClicked()
        {
            if (Manager.MapIsBeingBuilt) return;
            
            await Manager.CheckToSaveMapChanges();

            await EditorUIManager.MapSelectionDialog.Show(isModalClosingDialog: true);
        }

        private async void OnExitClicked()
        {
            if (await Manager.CheckToSaveMapChanges() == DialogBase.EConfirmResult.Cancel)
            {
                Manager.SetCurrentMapToOriginalMap();
            }
            
            Manager.LoadMainSceneClear();
        }
    }
}