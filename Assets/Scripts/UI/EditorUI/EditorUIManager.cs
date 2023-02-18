using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using Scripts.UI.EditorUI.PrefabEditors;
using Scripts.UI.Tooltip;
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
        private Transform _upperRightPanel;
        private InputDialog _inputDialog;
        private EntryPointEditor _entryPointEditor;
        private EditorStartPointEditor _editorStartPointEditor;
        private WallEditor _wallEditor;
        private TilePrefabEditor _tilePrefabEditor;
        private TriggerEditor _triggerEditor;
        private TriggerReceiverEditor _triggerReceiverEditor;
        private Transform _body;

        public TileGizmoController TileGizmo { get; private set; }
        public MessageBar MessageBar { get; private set; }
        public NewMapDialog NewMapDialog { get; private set; }
        public DialogBase ConfirmationDialog { get; private set; }
        public MapSelectionDialog MapSelectionDialog { get; private set; }
        public Cursor3D Cursor3D { get; private set; }
        public CageController SelectedCage { get; private set; }
        public SelectConfigurationWindow SelectConfigurationWindow { get; private set; }
        public TooltipController Tooltip { get; private set; }
        public EWorkMode WorkMode { get; private set; }
        public bool IsAnyObjectEdited { get; private set; }
        

        private ImageButton _playButton;
        private Title _mapTitle;

        public IPrefabEditor OpenedEditor;
        private Dictionary<EWorkMode, IPrefabEditor> _editors;

        protected override void Awake()
        {
            base.Awake();

            _body = transform.Find("Body");
            
            _upperRightPanel = _body.Find("UpperRightPanel");
            _upperRightPanel.gameObject.SetActive(false);
            _playButton = _upperRightPanel.Find("PlayButton").GetComponent<ImageButton>();
            _playButton.OnClick.AddListener(manager.PlayMap);
            _mapTitle = _upperRightPanel.Find("MapTitle").GetComponent<Title>();
            NewMapDialog = transform.Find("NewMapDialog").GetComponent<NewMapDialog>();
            ConfirmationDialog = transform.Find("ConfirmationDialog Variant").GetComponent<DialogBase>();
            MapSelectionDialog = transform.Find("MapSelectionDialog").GetComponent<MapSelectionDialog>();
            _inputDialog = transform.Find("InputDialog").GetComponent<InputDialog>();
            _entryPointEditor = _body.Find("EntryPointEditor").GetComponent<EntryPointEditor>();
            _editorStartPointEditor = _body.Find("EditorStartPointEditor").GetComponent<EditorStartPointEditor>();
            _wallEditor = _body.Find("WallEditor").GetComponent<WallEditor>();
            _tilePrefabEditor = _body.Find("TilePrefabEditor").GetComponent<TilePrefabEditor>();
            _triggerEditor = _body.Find("TriggerEditor").GetComponent<TriggerEditor>();
            _triggerReceiverEditor = _body.Find("TriggerReceiverEditor").GetComponent<TriggerReceiverEditor>();
            MessageBar = transform.Find("MessageBar").GetComponent<MessageBar>();
            Tooltip = transform.Find("Tooltip").GetComponent<TooltipController>();
            SelectedCage = _body.Find("SelectedCage").GetComponent<CageController>();
            SelectConfigurationWindow = _body.Find("SelectConfigurationWindow").GetComponent<SelectConfigurationWindow>();
            
            TileGizmo = FindObjectOfType<TileGizmoController>();
            Cursor3D = FindObjectOfType<Cursor3D>();
            
            _editors = new Dictionary<EWorkMode, IPrefabEditor>
            {
                {EWorkMode.EditEntryPoints, _entryPointEditor},
                {EWorkMode.EditEditorStart, _editorStartPointEditor},
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
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= OnNewMapStartedCreation;
            EditorEvents.OnWorkModeChanged -= OnWorkModeChanged;
        }

        private void OnNewMapStartedCreation()
        {
            string campaignName = GameManager.Instance.CurrentCampaign.CampaignName;
            string mapName = GameManager.Instance.CurrentMap.MapName;
            _mapTitle.Show($"{t.Get(Keys.Campaign)}: {campaignName.WrapInColor(Colors.Positive)} {t.Get(Keys.Map)}: {mapName.WrapInColor(Colors.Positive)}");
            
            _upperRightPanel.gameObject.SetActive(true);

            foreach (UIElementBase element in showOnMapLoad)
            {
                element.SetActive(true);
            }
        }

        private void OnWorkModeChanged(EWorkMode workMode)
        {
            WorkMode = workMode;
            IsAnyObjectEdited = false;
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
                case EPrefabType.TriggerOnWall:
                    _triggerEditor.Open(prefabType, placeholderTransformData);
                    OpenedEditor = _triggerEditor;
                    break;
                case EPrefabType.TriggerReceiver:
                case EPrefabType.Service:
                    if (WorkMode is EWorkMode.EditEntryPoints)
                    {
                        _entryPointEditor.Open(EPrefabType.Service, placeholderTransformData);
                        OpenedEditor = _entryPointEditor;
                    }
                    if (WorkMode is EWorkMode.EditEditorStart)
                    {
                        _editorStartPointEditor.Open(placeholderTransformData);
                        OpenedEditor = _editorStartPointEditor;
                    }
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
                case EPrefabType.TriggerOnWall:
                case EPrefabType.TriggerReceiver:
                case EPrefabType.Service:
                    if (WorkMode is EWorkMode.EditEntryPoints)
                    {
                        _entryPointEditor.Open(configuration as EntryPointConfiguration);
                        OpenedEditor = _entryPointEditor;
                    }
                    break;
                case EPrefabType.EntryPoint:
                default:
                    Logger.LogWarning($"Not implemented editor for type {configuration.PrefabType}.");
                    break;
            }
        }

        public void SetAnyObjectEdited(bool isEditing)
        {
            if (IsAnyObjectEdited == isEditing) return;
            
            IsAnyObjectEdited = isEditing;
            EditorEvents.TriggerOnPrefabEdited(isEditing);
        }

        private void CloseEditorWindow()
        {
            IsAnyObjectEdited = false;

            OpenedEditor?.CloseWithRemovingChanges();

            OpenedEditor = null;
        }

        public async Task<string> ShowInputFieldDialog(string title, string placeholder = "")
        {
            return await _inputDialog.Show(title, placeholder, "");
        }

        public async void ShowEditorUI()
        {
            await MapSelectionDialog.Show(false, false);
        }
    }
}