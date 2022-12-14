using Scripts.Building.Walls.Configurations;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : SingletonNotPersisting<EditorUIManager>
    {
        [SerializeField] private ImageButton playButton;
        [SerializeField] private WorkModeSelection workModeSelection;
        [SerializeField] private FloorManagement floorManagement;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private DialogBase confirmationDialog;
        [SerializeField] private WallEditorWindow wallEditor;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private Title mapTitle;
        [SerializeField] private MapEditorManager manager;

        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public DialogBase ConfirmationDialog => confirmationDialog;
        public bool IsAnyObjectEdited;

        private void OnEnable()
        {
            playButton.OnClick += manager.PlayMap;
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
        }

        private void OnDisable()
        {
            playButton.OnClick -= manager.PlayMap;
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
        }

        private void OnNewMapStartedCreation()
        {
            playButton.SetActive(true);
            workModeSelection.SetActive(true);
            floorManagement.SetActive(true);
            mapTitle.Show(GameManager.Instance.CurrentMap.MapName);
        }

        public void OpenTileEditorWindow(EWallType wallType, PositionRotation placeholderTransformData)
        {
            IsAnyObjectEdited = true;
            wallEditor.Open(wallType, placeholderTransformData);
        }

        public void OpenTileEditorWindow(WallConfiguration wallConfiguration)
        {
            IsAnyObjectEdited = true;
            wallEditor.Open(wallConfiguration);
        }

        public void CloseTileEditorWindow()
        {
            IsAnyObjectEdited = false;
            wallEditor.CloseWithChangeCheck();
        }
    }
}
