using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.MapEditor;
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
        [SerializeField] private MapEditorManager manager;
        private FileOperations _fileOperations;
        private NewMapDialog _newMapDialog;
        private DialogBase _confirmationDialog;
        private OpenFileDialog _openFileDialog;
        private WallEditor _wallEditor;
        private PrefabTileEditor _prefabTileEditor;
        private TriggerEditor _triggerEditor;
        private StatusBar _statusBar;
        private Transform _body;

        [NonSerialized] public WallGizmoController WallGizmo;
        public StatusBar StatusBar => _statusBar;
        public NewMapDialog NewMapDialog => _newMapDialog;
        public DialogBase ConfirmationDialog => _confirmationDialog;
        public OpenFileDialog OpenFileDialog => _openFileDialog;
        public bool isAnyObjectEdited;

        private ImageButton _playButton;
        private Title _mapTitle;

        private IPrefabEditor _openedEditor;

        protected override void Awake()
        {
            base.Awake();

            _body = transform.Find("Body");
            
            _playButton = _body.Find("PlayButton").GetComponent<ImageButton>();
            _playButton.OnClick.AddListener(manager.PlayMap);
            _mapTitle = _body.Find("MapTitle").GetComponent<Title>();
            _fileOperations = _body.Find("FileOperations").GetComponent<FileOperations>();
            _newMapDialog = transform.Find("NewMapDialog").GetComponent<NewMapDialog>();
            _confirmationDialog = transform.Find("ConfirmationDialog Variant").GetComponent<DialogBase>();
            _openFileDialog = transform.Find("OpenFileDialog").GetComponent<OpenFileDialog>();
            _wallEditor = _body.Find("WallEditor").GetComponent<WallEditor>();
            _prefabTileEditor = _body.Find("PrefabTileEditor").GetComponent<PrefabTileEditor>();
            _triggerEditor = _body.Find("TriggerEditor").GetComponent<TriggerEditor>();
            _statusBar = transform.Find("StatusBar").GetComponent<StatusBar>();
            
            WallGizmo = FindObjectOfType<WallGizmoController>();
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += OnNewMapStartedCreation;
            EditorEvents.OnWorkModeChanged += OnWorkModeChanged;

            _fileOperations.SetActive(true);
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
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

        private void OnEditingInterruptionImminent()
        {
            isAnyObjectEdited = false;
            CloseEditorWindow();
        }

        public void OpenEditorWindow(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            isAnyObjectEdited = true;

            switch (prefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    _wallEditor.Open(prefabType, placeholderTransformData);
                    _openedEditor = _wallEditor;
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
                    _prefabTileEditor.Open(prefabType, placeholderTransformData);
                    _openedEditor = _prefabTileEditor;
                    break;
                case EPrefabType.Trigger:
                    _triggerEditor.Open(prefabType, placeholderTransformData);
                    _openedEditor = _triggerEditor;
                    break;
                default:
                    isAnyObjectEdited = false;
                    Logger.LogWarning($"Not implemented editor for type {prefabType}.");
                    break;
            }
        }

        public void OpenEditorWindow(PrefabConfiguration configuration)
        {
            isAnyObjectEdited = true;

            switch (configuration.PrefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    _wallEditor.Open(configuration as WallConfiguration);
                    _openedEditor = _wallEditor;
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
                    _prefabTileEditor.Open(configuration as TilePrefabConfiguration);
                    _openedEditor = _prefabTileEditor;
                    break;
                default:
                    isAnyObjectEdited = false;
                    Logger.LogWarning($"Not implemented editor for type {configuration.PrefabType}.");
                    break;
            }
        }

        private void CloseEditorWindow()
        {
            isAnyObjectEdited = false;

            _openedEditor?.CloseWithRemovingChanges();

            _openedEditor = null;
        }
    }
}