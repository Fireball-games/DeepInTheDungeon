using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.PrefabEditors;
using UnityEngine;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : SingletonNotPersisting<EditorUIManager>
    {
        [SerializeField] private List<UIElementBase> showOnMapLoad;
        [SerializeField] private FileOperations fileOperations;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private DialogBase confirmationDialog;
        [SerializeField] private OpenFileDialog openFileDialog;
        [SerializeField] private WallEditor wallEditor;
        [SerializeField] private PrefabTileEditor prefabTileEditor;
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

        private IPrefabEditor _openedEditor;

        protected override void Awake()
        {
            base.Awake();

            _playButton = body.transform.Find("PlayButton").GetComponent<ImageButton>();
            _mapTitle = body.transform.Find("MapTitle").GetComponent<Title>();
            WallGizmo = FindObjectOfType<WallGizmoController>();
        }

        private void OnEnable()
        {
            _playButton.OnClick += manager.PlayMap;

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
            CloseEditorWindow();
        }

        public void OpenEditorWindow(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            IsAnyObjectEdited = true;

            switch (prefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    wallEditor.Open(prefabType, placeholderTransformData);
                    _openedEditor = wallEditor;
                    break;
                case EPrefabType.Invalid:
                    break;
                case EPrefabType.Enemy:
                    break;
                case EPrefabType.Prop:
                    break;
                case EPrefabType.Item:
                    break;
                case EPrefabType.PrefabTile:
                    prefabTileEditor.Open(prefabType, placeholderTransformData);
                    _openedEditor = prefabTileEditor;
                    break;
                default:
                    Logger.LogWarning($"Not implemented editor for type {prefabType}.");
                    break;
            }
        }

        public void OpenEditorWindow(PrefabConfiguration configuration)
        {
            IsAnyObjectEdited = true;

            switch (configuration.PrefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    wallEditor.Open(configuration as WallConfiguration);
                    _openedEditor = wallEditor;
                    break;
                case EPrefabType.Invalid:
                    break;
                case EPrefabType.Enemy:
                    break;
                case EPrefabType.Prop:
                    break;
                case EPrefabType.Item:
                    break;
                case EPrefabType.PrefabTile:
                    prefabTileEditor.Open(configuration as TilePrefabConfiguration);
                    _openedEditor = prefabTileEditor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CloseEditorWindow()
        {
            IsAnyObjectEdited = false;

            _openedEditor?.CloseWithChangeCheck();

            _openedEditor = null;
        }
    }
}