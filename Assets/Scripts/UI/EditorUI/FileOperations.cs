using System.IO;
using System.Linq;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.UI;

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
            exitButton.onClick.AddListener(OnExitClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            newMapButton.onClick.AddListener(OnNewMapClicked);
        }
        
        private static string GetDefaultMapName()
        {
            string newMapName = T.Get(LocalizationKeys.NewMap);
            
            string[] fileNames = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.MapDirectory);

            fileNames = fileNames.Select(Path.GetFileName).ToArray();

            int number = 1;

            while (fileNames.Contains($"{T.Get(LocalizationKeys.NewMap)}{number}.map"))
            {
                number++;
            }

            return $"{newMapName}{number}";
        }
        
        private void OnLoadClicked()
        {
            _existingFiles = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.MapDirectory);

            if (_existingFiles == null || !_existingFiles.Any())
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(LocalizationKeys.NoFilesToShow),
                    StatusBar.EMessageType.Warning);
                return;
            }
            
            openFileDialog.Open(T.Get(LocalizationKeys.SelectMapToLoad), _existingFiles, LoadMap);
        }
        
        private void OnSaveClicked()
        {
            if (!Manager.MapIsSaved)
            {
                Manager.SaveMap();
                return;
            }

            EditorUIManager.Instance.StatusBar.RegisterMessage(
                    T.Get(LocalizationKeys.NoChangesToSave),
                    StatusBar.EMessageType.Warning);
        }
        
        private void OnNewMapClicked()
        {
            if (MapEditorManager.MapIsBeingBuilt) return;

            if (Manager.MapIsChanged || !Manager.MapIsSaved)
            {
                EditorUIManager.Instance.ConfirmationDialog.Open(
                    T.Get(LocalizationKeys.SaveEditedMapPrompt),
                    OpenNewMapDialogWithSave,
                    OpenNewMapDialog,
                    T.Get(LocalizationKeys.SaveMap),
                    T.Get(LocalizationKeys.DontSave)
                    );
                return;
            }

            OpenNewMapDialog();
        }

        private void OpenNewMapDialogWithSave()
        {
            Manager.SaveMap();
            OpenNewMapDialog();
        }

        private void OpenNewMapDialog()
        {
            EditorUIManager.Instance.NewMapDialog.Open(T.Get(LocalizationKeys.NewMapDialogTitle),
                GetDefaultMapName(),
                OnNewMapDialogOK
            );
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
            MapDescription loadedMap = ES3.Load<MapDescription>(Path.GetFileNameWithoutExtension(filePath), filePath);
            Manager.OrderMapConstruction(loadedMap, true);
        }
    }
}
