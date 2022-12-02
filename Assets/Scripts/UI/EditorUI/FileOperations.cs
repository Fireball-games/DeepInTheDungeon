using System;
using System.Linq;
using Scripts.Helpers;
using Scripts.Localization;
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
        [SerializeField] private OpenFileDialogController openFileDialog;

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

            if (_existingFiles == null || !_existingFiles.Any())
            {
                EditorUIManager.StatusBar.RegisterMessage(GetTranslationText(LocalizationKeys.NoFilesToShow), StatusBar.EMessageType.None);
                return;
            }
            
            openFileDialog.Open(GetTranslationText(LocalizationKeys.SelectMapToLoad), _existingFiles, LoadMap);
        }
        
        private void OnSaveClicked()
        {
            throw new NotImplementedException();
        }
        
        private void OnNewMapClicked()
        {
            throw new NotImplementedException();
        }

        private void OnExitClicked()
        {
            throw new NotImplementedException();
        }

        private void LoadMap(string mapName)
        {
            Logger.Log($"Loading file: {mapName}");
        }
    }
}
