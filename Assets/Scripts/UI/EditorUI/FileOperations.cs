using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MessageBar;
using static Scripts.Helpers.Strings;
using static Scripts.System.MonoBases.DialogBase;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI
{
    public class FileOperations : UIElementBase
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button newMapButton;

        private static GameManager GameManager => GameManager.Instance;
        private static MapEditorManager EditorManager => MapEditorManager.Instance;
        private static EditorUIManager EditorUIManager => EditorUIManager.Instance;

        private void OnEnable()
        {
            loadButton.onClick.AddListener(OnLoadClicked);
            loadButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Load);
            exitButton.onClick.AddListener(OnExitClicked);
            exitButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Exit);
            saveButton.onClick.AddListener(OnSaveClicked);
            saveButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Save);
            newMapButton.onClick.AddListener(OnNewMapClicked);
            newMapButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.NewMap);
        }

        private async void OnLoadClicked()
        {
            if (EditorManager.MapIsChanged || !EditorManager.MapIsSaved && await OpenConfirmationDialog() is EConfirmResult.Ok)
            {
                EditorManager.SaveMap();
            }


            EditorUIManager.MapSelectionDialog.Show();
        }

        private void OnSaveClicked()
        {
            if (!EditorManager.MapIsSaved)
            {
                EditorManager.SaveMap();
                return;
            }

            EditorUIManager.MessageBar.Set(
                t.Get(Keys.NoChangesToSave),
                EMessageType.Warning, automaticDismissDelay: 1f);
        }

        private async void OnNewMapClicked()
        {
            if (EditorManager.MapIsBeingBuilt) return;

            if (EditorManager.MapIsChanged || !EditorManager.MapIsSaved && await OpenConfirmationDialog() == EConfirmResult.Ok)
            {
                EditorManager.SaveMap();
            }

            string defaultMapName = GetDefaultName(
                t.Get(Keys.NewMapName),
                GameManager.CurrentCampaign.Maps.Select(m => m.MapName)
            );

            if (await EditorUIManager.NewMapDialog.Show(t.Get(Keys.NewMapDialogTitle), defaultMapName) == EConfirmResult.Ok)
            {
                OnNewMapDialogOK();
            }
        }

        private async Task<EConfirmResult> OpenConfirmationDialog() =>
            await EditorUIManager.ConfirmationDialog.Show(
                t.Get(Keys.SaveEditedMapPrompt),
                t.Get(Keys.SaveMap),
                t.Get(Keys.DontSave)
            );

        private void OnNewMapDialogOK()
        {
            NewMapDialog dialog = EditorUIManager.Instance.NewMapDialog;
            int rows = int.Parse(dialog.rowsInput.Text);
            int columns = int.Parse(dialog.columnsInput.Text);
            int floors = int.Parse(dialog.floorsInput.Text) + 2;
            string mapName = dialog.mapNameInput.Text;

            MapDescription newMap = MapBuilder.GenerateDefaultMap(
                Mathf.Clamp(floors, MapEditorManager.MinFloors, MapEditorManager.MaxFloors),
                Mathf.Clamp(rows, MapEditorManager.MinRows, MapEditorManager.MaxRows),
                Mathf.Clamp(columns, MapEditorManager.MinColumns, MapEditorManager.MaxColumns));

            newMap.MapName = string.IsNullOrEmpty(mapName)
                ? GetDefaultName(
                    t.Get(Keys.NewMapName),
                    GameManager.CurrentCampaign.Maps.Select(m => m.MapName)
                )
                : mapName;

            EditorManager.OrderMapConstruction(newMap);
        }

        private void OnExitClicked()
        {
            EditorManager.GoToMainMenu();
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
                EditorUIManager.MessageBar.Set(t.Get(Keys.LoadingFileFailed), EMessageType.Negative, automaticDismissDelay: 3f);
                return;
            }

            EditorUIManager.MapSelectionDialog.CloseDialog();

            EditorManager.OrderMapConstruction(loadedMap, true);
        }
    }
}