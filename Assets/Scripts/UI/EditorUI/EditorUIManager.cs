using System;
using System.Runtime.CompilerServices;
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

        [NonSerialized] public WallGizmoController WallGizmo;
        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public DialogBase ConfirmationDialog => confirmationDialog;
        public bool IsAnyObjectEdited;

        protected override void Awake()
        {
            base.Awake();

            WallGizmo = FindObjectOfType<WallGizmoController>();
        }

        private void OnEnable()
        {
            playButton.OnClick += manager.PlayMap;
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged += OnFloorChanged;
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
        }

        private void OnDisable()
        {
            playButton.OnClick -= manager.PlayMap;
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged -= OnFloorChanged;
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
        }

        private void OnNewMapStartedCreation()
        {
            playButton.SetActive(true);
            workModeSelection.SetActive(true);
            floorManagement.SetActive(true);
            mapTitle.Show(GameManager.Instance.CurrentMap.MapName);
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

        public void CloseWallEditorWindow()
        {
            IsAnyObjectEdited = false;
            wallEditor.CloseWithChangeCheck();
        }
    }
}
