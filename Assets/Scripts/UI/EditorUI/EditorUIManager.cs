using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
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
        private TilePrefabEditor _tilePrefabEditor;
        private TriggerEditor _triggerEditor;
        private TriggerReceiverEditor _triggerReceiverEditor;
        private StatusBar _statusBar;
        private CageController _selectedCage;
        private SelectConfigurationWindow _selectConfiguration;
        private Transform _body;
        private Cursor3D _cursor3D;

        [NonSerialized] public WallGizmoController WallGizmo;
        public StatusBar StatusBar => _statusBar;
        public NewMapDialog NewMapDialog => _newMapDialog;
        public DialogBase ConfirmationDialog => _confirmationDialog;
        public OpenFileDialog OpenFileDialog => _openFileDialog;
        public Cursor3D Cursor3D => _cursor3D;
        public CageController SelectedCage => _selectedCage;
        public SelectConfigurationWindow SelectConfigurationWindow => _selectConfiguration;
        public bool isAnyObjectEdited { get; private set; }

        private ImageButton _playButton;
        private Title _mapTitle;

        public IPrefabEditor OpenedEditor;
        private Dictionary<EWorkMode, IPrefabEditor> _editors;

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
            _tilePrefabEditor = _body.Find("PrefabTileEditor").GetComponent<TilePrefabEditor>();
            _triggerEditor = _body.Find("TriggerEditor").GetComponent<TriggerEditor>();
            _triggerReceiverEditor = _body.Find("TriggerReceiverEditor").GetComponent<TriggerReceiverEditor>();
            _statusBar = transform.Find("StatusBar").GetComponent<StatusBar>();
            _selectedCage = _body.Find("SelectedCage").GetComponent<CageController>();
            _selectConfiguration = _body.Find("SelectConfigurationWindow").GetComponent<SelectConfigurationWindow>();
            
            WallGizmo = FindObjectOfType<WallGizmoController>();
            _cursor3D = FindObjectOfType<Cursor3D>();
            
            _editors = new Dictionary<EWorkMode, IPrefabEditor>
            {
                {EWorkMode.Walls, _wallEditor},
                {EWorkMode.PrefabTiles, _tilePrefabEditor},
                {EWorkMode.Triggers, _triggerEditor},
                {EWorkMode.TriggerReceivers, _triggerReceiverEditor}
            };
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

        private void OnWorkModeChanged(EWorkMode workMode)
        {
            isAnyObjectEdited = false;
            CloseEditorWindow();
            
            if (_editors.TryGetValue(workMode, out IPrefabEditor editor))
            {
                OpenedEditor = editor;
                editor.Open();
            }
        }

        public void OpenEditorWindow(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            switch (prefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    _wallEditor.Open(prefabType, placeholderTransformData);
                    OpenedEditor = _wallEditor;
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
                    _tilePrefabEditor.Open(prefabType, placeholderTransformData);
                    OpenedEditor = _tilePrefabEditor;
                    break;
                case EPrefabType.Trigger:
                    _triggerEditor.Open(prefabType, placeholderTransformData);
                    OpenedEditor = _triggerEditor;
                    break;
                default:
                    Logger.LogWarning($"Not implemented editor for type {prefabType}.");
                    break;
            }
        }

        public void OpenEditorWindow(PrefabConfiguration configuration)
        {
            switch (configuration.PrefabType)
            {
                case EPrefabType.Wall:
                case EPrefabType.WallBetween:
                case EPrefabType.WallOnWall:
                case EPrefabType.WallForMovement:
                    _wallEditor.Open(configuration as WallConfiguration);
                    OpenedEditor = _wallEditor;
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
                    _tilePrefabEditor.Open(configuration as TilePrefabConfiguration);
                    OpenedEditor = _tilePrefabEditor;
                    break;
                default:
                    Logger.LogWarning($"Not implemented editor for type {configuration.PrefabType}.");
                    break;
            }
        }

        public void SetAnyObjectEdited(bool isEditing)
        {
            if (isAnyObjectEdited == isEditing) return;
            
            isAnyObjectEdited = isEditing;
            EditorEvents.TriggerOnPrefabEdited(isEditing);
        }

        private void CloseEditorWindow()
        {
            isAnyObjectEdited = false;

            OpenedEditor?.CloseWithRemovingChanges();

            OpenedEditor = null;
        }
    }
}