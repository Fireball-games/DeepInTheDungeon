using System.Linq;
using Scripts.Building;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;
using static Lean.Localization.LeanLocalization;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI
{
    public class FileOperations : MonoBehaviour
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button newMapButton;
        [SerializeField] private OpenFileDialog openFileDialog;
        [SerializeField] private MapEditorManager editorManager;

        private string[] _existingFiles;

        private void OnEnable()
        {
            loadButton.onClick.AddListener(OnLoadClicked);
            exitButton.onClick.AddListener(OnExitClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            newMapButton.onClick.AddListener(OnNewMapClicked);
        }
        private void OnLoadClicked()
        {
            _existingFiles = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.MapDirectory);

            _existingFiles = new[]{ "File 1", "File 2"};
            
            if (_existingFiles == null || !_existingFiles.Any())
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(LocalizationKeys.NoFilesToShow),
                    StatusBar.EMessageType.Warning);
                return;
            }
            
            openFileDialog.Open(GetTranslationText(LocalizationKeys.SelectMapToLoad), _existingFiles, LoadMap);
        }
        
        private void OnSaveClicked()
        {
            if (!editorManager.MapIsChanged)
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(LocalizationKeys.NoChangesToSave),
                    StatusBar.EMessageType.Warning);
            }
        }
        
        private void OnNewMapClicked()
        {
            if (editorManager.MapIsBeingBuilt) return;
            
            if (!editorManager.MapIsEdited)
            {
                EditorUIManager.Instance.NewMapDialog.Open(T.Get(LocalizationKeys.NewMapDialogTitle),
                    OnNewMapDialogOK);
                // editorManager.OrderMapConstruction();
                return;
            }
            
            //TODO: confirmation dialog if to save, discard current map or cancel.
        }

        private void OnExitClicked()
        {
            Logger.LogWarning("NOT IMPLEMENTED YET");
        }

        private void LoadMap(string mapName)
        {
            Logger.Log($"Loading file: {mapName}");
        }

        private void OnNewMapDialogOK()
        {
            NewMapDialog dialog = EditorUIManager.Instance.NewMapDialog;
            int rows = int.Parse(dialog.rowsInput.Text);
            int columns = int.Parse(dialog.columnsInput.Text);
            string mapName = dialog.mapNameInput.Text;

            MapDescription newMap = MapBuilder.GenerateDefaultMap(
                Mathf.Max(rows, MapEditorManager.MinRows), Mathf.Max(columns, MapEditorManager.MinColumns));

            newMap.MapName = string.IsNullOrEmpty(mapName) 
                ? T.Get(LocalizationKeys.NewMap) 
                : mapName;
            
            editorManager.OrderMapConstruction(newMap);
        }
    }
}
