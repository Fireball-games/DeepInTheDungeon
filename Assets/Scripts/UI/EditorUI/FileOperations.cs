using System;
using System.IO;
using System.Linq;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

        private static MapEditorManager Manager => MapEditorManager.Instance;
        private string[] _existingFiles;

        private void OnEnable()
        {
            loadButton.onClick.AddListener(OnLoadClicked);
            loadButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Load);
            exitButton.onClick.AddListener(OnExitClicked);
            exitButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Exit);
            saveButton.onClick.AddListener(OnSaveClicked);
            saveButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Save);
            newMapButton.onClick.AddListener(OnNewMapClicked);
            newMapButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.NewMap);
        }
        
        private static string GetDefaultMapName()
        {
            string newMapName = T.Get(Keys.NewMap);
            
            string[] fileNames = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.MapDirectoryName);

            fileNames ??= new string[]{};

            fileNames = fileNames.Select(Path.GetFileName).ToArray();

            int number = 1;

            while (fileNames.Contains($"{T.Get(Keys.NewMap)}{number}.map"))
            {
                number++;
            }

            return $"{newMapName}{number}";
        }
        
        private void OnLoadClicked()
        {
            _existingFiles = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.MapDirectoryName);

            if (_existingFiles == null || !_existingFiles.Any())
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(Keys.NoFilesToShow),
                    StatusBar.EMessageType.Warning);
                return;
            }

            if (Manager.MapIsChanged || !Manager.MapIsSaved)
            {
                OpenConfirmationDialog(LoadMapConfirmedWithSave, LoadMapConfirmed);
                return;
            }
            
            LoadMapConfirmed();
        }

        private void LoadMapConfirmedWithSave()
        {
            Manager.SaveMap();
            LoadMapConfirmed();
        }

        private void LoadMapConfirmed()
        {
            openFileDialog.Open(T.Get(Keys.SelectMapToLoad), _existingFiles, LoadMap);
        }
        
        private void OnSaveClicked()
        {
            if (!Manager.MapIsSaved)
            {
                Manager.SaveMap();
                return;
            }

            EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(Keys.NoChangesToSave),
                    StatusBar.EMessageType.Warning);
        }
        
        private void OnNewMapClicked()
        {
            if (MapEditorManager.Instance.MapIsBeingBuilt) return;

            if (Manager.MapIsChanged || !Manager.MapIsSaved)
            {
                OpenConfirmationDialog(OpenNewMapDialogWithSave, OpenNewMapDialog);
                return;
            }

            OpenNewMapDialog();
        }

        private void OpenConfirmationDialog(Action onOk, Action onCancel)
        {
            EditorUIManager.Instance.ConfirmationDialog.Open(
                T.Get(Keys.SaveEditedMapPrompt),
                onOk,
                onCancel,
                T.Get(Keys.SaveMap),
                T.Get(Keys.DontSave)
            );
        }

        private void OpenNewMapDialogWithSave()
        {
            Manager.SaveMap();
            OpenNewMapDialog();
        }

        private void OpenNewMapDialog()
        {
            EditorUIManager.Instance.NewMapDialog.Open(T.Get(Keys.NewMapDialogTitle),
                GetDefaultMapName(),
                OnNewMapDialogOK
            );
        }
        
        private void OnNewMapDialogOK()
        {
            NewMapDialog dialog = EditorUIManager.Instance.NewMapDialog;
            int rows = int.Parse(dialog.rowsInput.Text);
            int columns = int.Parse(dialog.columnsInput.Text);
            int floors = int.Parse(dialog.floorsInput.Text);
            string mapName = dialog.mapNameInput.Text;

            MapDescription newMap = MapBuilder.GenerateDefaultMap(
                Mathf.Clamp(floors, MapEditorManager.MinFloors, MapEditorManager.MaxFloors),
                Mathf.Clamp(rows, MapEditorManager.MinRows, MapEditorManager.MaxRows),
                Mathf.Clamp(columns, MapEditorManager.MinColumns, MapEditorManager.MaxColumns));

            newMap.MapName = string.IsNullOrEmpty(mapName) 
                ? GetDefaultMapName()
                : mapName;
            
            Manager.OrderMapConstruction(newMap);
        }

        private void OnExitClicked()
        {
            Manager.GoToMainMenu();
        }

        private void LoadMap(string filePath)
        {
            MapDescription loadedMap = null;
                
            try
            {
                loadedMap = ES3.Load<MapDescription>(Path.GetFileNameWithoutExtension(filePath), filePath);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load map \"{filePath}\" : {e.Message}.", Logger.ELogSeverity.Release);
            }

            if (loadedMap == null)
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(T.Get(Keys.LoadingFileFailed), StatusBar.EMessageType.Negative);
                return;
            }
            
            openFileDialog.CloseDialog();
            Manager.OrderMapConstruction(loadedMap, true);
        }
    }
}
