using System;
using System.Collections.Generic;
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
        [SerializeField] private List<UIElementBase> showOnMapLoad;
        [SerializeField] private FileOperations fileOperations;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private DialogBase confirmationDialog;
        [SerializeField] private OpenFileDialog openFileDialog;
        [SerializeField] private WallEditorWindow wallEditor;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private MapEditorManager manager;
        [SerializeField] private GameObject body;

        [NonSerialized] public WallGizmoController WallGizmo;
        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public DialogBase ConfirmationDialog => confirmationDialog;
        public OpenFileDialog OpenFileDialog => openFileDialog;
        public bool IsAnyObjectEdited;

        private ImageButton _playButton;
        private Title _mapTitle;

        protected override void Awake()
        {
            base.Awake();

            _playButton = body.transform.Find("PlayButton").GetComponent<ImageButton>();
            _mapTitle = body.transform.Find("MapTitle").GetComponent<Title>();
            WallGizmo = FindObjectOfType<WallGizmoController>();
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged += OnFloorChanged;
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            
            fileOperations.SetActive(true);
        }

        private void OnDisable()
        {
            _playButton.OnClick -= manager.PlayMap;
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged -= OnFloorChanged;
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
        }

        private void OnNewMapStartedCreation()
        {
            _mapTitle.Show(GameManager.Instance.CurrentMap.MapName);

            foreach (UIElementBase element in showOnMapLoad)
            {
                element.SetActive(true);
            }
        }

        private void OnWorkModeChanged(EWorkMode _) => OnEditingInterruptionImminent();

        private void OnFloorChanged(int? _) => OnEditingInterruptionImminent();

        private void OnEditingInterruptionImminent()
        {
            IsAnyObjectEdited = false;
            CloseWallEditorWindow();
        } 

        public void OpenWallEditorWindow(EWallType wallType, PositionRotation placeholderTransformData)
        {
            IsAnyObjectEdited = true;
            wallEditor.Open(wallType, placeholderTransformData);
        }

        public void OpenWallEditorWindow(WallConfiguration wallConfiguration)
        {
            IsAnyObjectEdited = true;
            wallEditor.Open(wallConfiguration);
        }

        private void CloseWallEditorWindow()
        {
            IsAnyObjectEdited = false;
            wallEditor.CloseWithChangeCheck();
        }
    }
}
