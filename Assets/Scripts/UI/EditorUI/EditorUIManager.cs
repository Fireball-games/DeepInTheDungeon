using System;
using System.Collections.Generic;
using Scripts.Building.Walls.Configurations;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.Enums;
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
        private ToggleFramedButton _perspectiveToggle;

        protected override void Awake()
        {
            base.Awake();

            _playButton = body.transform.Find("PlayButton").GetComponent<ImageButton>();
            _mapTitle = body.transform.Find("MapTitle").GetComponent<Title>();
            _perspectiveToggle = body.transform.Find("PerspectiveToggle").GetComponent<ToggleFramedButton>();
            WallGizmo = FindObjectOfType<WallGizmoController>();
        }

        private void OnEnable()
        {
            _playButton.OnClick += manager.PlayMap;
            _perspectiveToggle.OnClick += OnPerspectiveToggleClick;
            _perspectiveToggle.dontToggleOnclick = true;
            
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged += OnFloorChanged;
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;
            EditorEvents.OnCameraPerspectiveChanged += OnCameraPerspectiveChanged;
            
            fileOperations.SetActive(true);
        }

        private void OnDisable()
        {
            _playButton.OnClick -= manager.PlayMap;
            _perspectiveToggle.OnClick -= OnPerspectiveToggleClick;
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnFloorChanged -= OnFloorChanged;
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
            EditorEvents.OnCameraPerspectiveChanged += OnCameraPerspectiveChanged;
        }

        private void OnCameraPerspectiveChanged(bool isOrthographic)
        {
            if (isOrthographic)
                _perspectiveToggle.ToggleOff(true);
            else
                _perspectiveToggle.ToggleOn(true);
        }

        private void OnPerspectiveToggleClick()
        {
            EditorCameraService.ToggleCameraPerspective();
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

        public void OpenWallEditorWindow(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            IsAnyObjectEdited = true;
            wallEditor.Open(prefabType, placeholderTransformData);
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
