using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public abstract class PrefabEditorBase<TC, TPrefab, TService> : EditorWindowBase, IPrefabEditor
        where TC : PrefabConfiguration
        where TPrefab : PrefabBase
        where TService : IPrefabService<TC>, new()
    {
        protected GameObject Placeholder;
        protected Transform Content;
        protected IPrefabService<TC> Service;

        private PrefabList _prefabList;
        private ConfigurationList _existingList;
        private GameObject _mainWindow;
        private Button _saveButton;
        private Button _cancelButton;
        private Button _deleteButton;
        private Button _closeButton;
        private ImageButton _prefabsFinderButton;
        private Title _prefabTitle;
        private TMP_Text _statusText;
        private EditorUIManager Manager => EditorUIManager.Instance;

        protected CageController SelectedCage => Manager.SelectedCage;
        protected MapBuilder MapBuilder => MapEditorManager.Instance.MapBuilder;
        protected TC EditedConfiguration;
        protected TPrefab EditedPrefab;
        protected EPrefabType EditedPrefabType;
        protected GameObject PhysicalPrefabBody;
        protected GameObject PhysicalPrefab;
        protected bool IsCurrentConfigurationChanged;

        private TC _originalConfiguration;
        private HashSet<TPrefab> _availablePrefabs;
        private bool _isEditingExistingPrefab;

        protected bool CanOpen => !IsCurrentConfigurationChanged;
        protected bool IsPrefabFinderActive => _existingList.IsActive;

        private void Awake()
        {
            Service = new TService();
            
            AssignComponents();
            InitializeOtherComponents();

            _closeButton.onClick.AddListener(RemoveAndReopen);
            _saveButton.onClick.AddListener(SaveMap);
            _deleteButton.onClick.AddListener(Delete);
            _cancelButton.onClick.AddListener(RemoveChanges);
            _prefabsFinderButton.OnClick.AddListener(Open);
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += Close;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= Close;
            StopAllCoroutines();
        }

        protected abstract TC GetNewConfiguration(string prefabName);
        protected abstract TC CloneConfiguration(TC sourceConfiguration);
        protected abstract void VisualizeOtherComponents();
        protected abstract void InitializeOtherComponents();

        /// <summary>
        /// Method to remove other components inheriting editor needs, should not contain any other manipulation than with other components.  
        /// </summary>
        protected abstract void RemoveOtherComponents();

        public virtual Vector3 GetCursor3DScale() => Vector3.one;

        /// <summary>
        /// Opens Existing Prefabs list. Next steps are - clicking in map to add/edit prefabs or open prefabs finder.
        /// </summary>
        public virtual void Open()
        {
            _mainWindow.SetActive(false);
            _prefabList.Close();
            _existingList.Close();
            _prefabTitle.SetActive(false);
            EditedConfiguration = _originalConfiguration = null;
            SetEdited(false);
            SelectedCage.Hide();

            SetButtons();

            IEnumerable<TC> availableConfigurations = Service.GetConfigurations();//GetAvailableConfigurations();

            SetExistingList(true, availableConfigurations);

            SetActive(true);
        }

        public virtual void Open(TC configuration)
        {
            if (!CanOpen) return;

            if (configuration == null)
            {
                return;
            }

            RemoveOtherComponents();

            _isEditingExistingPrefab = true;
            IsCurrentConfigurationChanged = false;
            EditedPrefabType = configuration.PrefabType;
            EditedConfiguration = configuration;
            _originalConfiguration = CloneConfiguration(configuration);

            SetupWindow(configuration.PrefabType);

            MoveCameraToPrefab(configuration.TransformData.Position);

            SelectedCage.ShowAt(configuration.TransformData.Position,
                GetCursor3DScale(),
                configuration.TransformData.Rotation);

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(configuration.PrefabName);

            SetExistingList(false);
            SetPrefabList(EditedConfiguration.SpawnPrefabOnBuild, _availablePrefabs!);

            PhysicalPrefab = Service.GetGameObject(EditedConfiguration.Guid);
            
            if (PhysicalPrefab)
            {
                EditedPrefab = !EditedConfiguration.SpawnPrefabOnBuild 
                    ? PhysicalPrefab.GetComponentsInChildren<TPrefab>()
                        .FirstOrDefault(p => p.gameObject.name == EditedConfiguration.PrefabName) 
                    : PhysicalPrefab.GetComponent<TPrefab>();

                if (EditedPrefab)
                {
                    PhysicalPrefabBody = EditedPrefab.gameObject.GetBody()?.gameObject;
                }
            }

            VisualizeOtherComponents();
        }

        public void Open(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            if (!CanOpen) return;

            MoveCameraToPrefab(placeholderTransformData.Position);

            _isEditingExistingPrefab = false;
            EditedConfiguration = null;

            SetupWindow(prefabType);

            Placeholder.transform.position = placeholderTransformData.Position;
            Placeholder.transform.rotation = placeholderTransformData.Rotation;
            Placeholder.transform.parent = null;
            Placeholder.SetActive(true);

            if (_availablePrefabs == null || !_availablePrefabs.Any())
            {
                EditedPrefabType = EPrefabType.Invalid;
                SetStatusText(t.Get(Keys.NoPrefabsAvailable));
                return;
            }

            SelectedCage.ShowAt(placeholderTransformData.Position,
                GetCursor3DScale(),
                placeholderTransformData.Rotation);

            EditedPrefabType = prefabType;

            SetStatusText(t.Get(Keys.SelectPrefab));

            VisualizeOtherComponents();
            SetExistingList(false);
            SetPrefabList(true, _availablePrefabs);
        }

        /// <summary>
        /// IPrefabEditor method
        /// </summary>
        public virtual void CloseWithRemovingChanges()
        {
            RemoveAndClose();
        }

        public virtual void MoveCameraToPrefab(Vector3 targetPosition) =>
            EditorCameraService.Instance.MoveCameraToPrefab(Vector3Int.RoundToInt(targetPosition));

        private void SetPrefab(string prefabName)
        {
            RemoveOtherComponents();
            SetStatusText();

            if (EditedConfiguration != null)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                EditedConfiguration.PrefabName = prefabName;
                EditedConfiguration = CloneConfiguration(EditedConfiguration);
            }
            else
            {
                EditedConfiguration = GetNewConfiguration(prefabName);
            }

            if (!MapBuilder.BuildPrefab(EditedConfiguration))
            {
                SetStatusText(t.Get(Keys.ErrorBuildingPrefab));
                Placeholder.SetActive(false);
                return;
            }

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(EditedConfiguration.PrefabName);

            if (!EditedConfiguration.SpawnPrefabOnBuild) SetPrefabList(false);

            PhysicalPrefab = MapBuilder.GetPrefabByGuid(EditedConfiguration.Guid);

            if (PhysicalPrefab)
            {
                EditedPrefab = PhysicalPrefab.GetComponent<TPrefab>();
                PhysicalPrefabBody = PhysicalPrefab.GetBody()?.gameObject;
            }

            SetEdited();
            SetButtons();
            Placeholder.SetActive(false);

            VisualizeOtherComponents();
        }

        protected virtual IEnumerable<TC> GetAvailableConfigurations() => MapBuilder.GetConfigurationsByPrefabClass<TC, TPrefab>();

        protected void SetEdited(bool isEdited = true)
        {
            IsCurrentConfigurationChanged = isEdited;
            Manager.SetAnyObjectEdited(isEdited);
            SetButtons();
            
            EditorEvents.TriggerOnPrefabEdited(isEdited);
        }

        protected virtual void Delete()
        {
            MapBuilder.RemovePrefab(EditedConfiguration);

            MapEditorManager.Instance.SaveMap();
            _prefabList.DeselectButtons();
            _isEditingExistingPrefab = false;
            EditedConfiguration = null;
            EditedPrefab = null;
            _originalConfiguration = null;
            SetEdited(false);
            _prefabList.DeselectButtons();

            Placeholder.SetActive(false);

            SetButtons();
            VisualizeOtherComponents();
        }

        protected virtual void RemoveAndReopen()
        {
            if (IsCurrentConfigurationChanged)
            {
                RemoveChanges();
            }

            Placeholder.SetActive(false);

            Close();
            Open();
        }

        private void RemoveAndClose()
        {
            if (IsCurrentConfigurationChanged)
            {
                RemoveChanges();
            }

            Close();
        }

        private void RemoveChanges()
        {
            if (_isEditingExistingPrefab)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                MapBuilder.BuildPrefab(_originalConfiguration);
                EditedConfiguration = CloneConfiguration(_originalConfiguration);
            }

            if (!_isEditingExistingPrefab && EditedConfiguration != null)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                EditedConfiguration = null;
                EditedPrefab = null;
                _prefabList.DeselectButtons();
            }

            SetEdited(false);
            SetButtons();
            VisualizeOtherComponents();
        }

        protected virtual void SaveMap()
        {
            SetEdited(false);

            _isEditingExistingPrefab = true;
            MapBuilder.AddReplacePrefabConfiguration(EditedConfiguration);
            MapEditorManager.Instance.SaveMap();

            SetButtons();
        }

        protected void Close()
        {
            EditedConfiguration = null;
            EditedPrefab = null;
            _originalConfiguration = null;
            _isEditingExistingPrefab = false;
            EditedPrefabType = EPrefabType.Invalid;

            Placeholder.transform.position = Vector3.zero;
            Placeholder.transform.parent = body.transform;
            Placeholder.SetActive(false);

            _prefabTitle.SetActive(false);
            SetPrefabList(false);
            SetExistingList(false);
            _mainWindow.SetActive(false);
            
            PhysicalPrefabBody = null;
            PhysicalPrefab = null;

            SelectedCage.Hide();

            Manager.SetAnyObjectEdited(false);
            Manager.WallGizmo.Reset();
            EditorMouseService.Instance.RefreshMousePosition();

            SetActive(false);
        }


        private void SetButtons()
        {
            _cancelButton.gameObject.SetActive(IsCurrentConfigurationChanged);
            _deleteButton.gameObject.SetActive(_isEditingExistingPrefab && EditedConfiguration is {SpawnPrefabOnBuild: true});
            _saveButton.gameObject.SetActive(IsCurrentConfigurationChanged);
            _closeButton.SetTextColor(IsCurrentConfigurationChanged ? Colors.Negative : Colors.White);
            _prefabsFinderButton.SetInteractable(!IsCurrentConfigurationChanged);
        }

        private void SetStatusText(string text = null)
        {
            _statusText.text = text ?? "";

            if (string.IsNullOrEmpty(text))
            {
                _statusText.gameObject.SetActive(false);
                return;
            }

            _statusText.gameObject.SetActive(true);
        }

        private void SetupWindow(EPrefabType prefabType)
        {
            SetActive(true);
            _prefabList.SetActive(false);
            SetExistingList(false);
            _mainWindow.SetActive(true);
            SetStatusText();

            _closeButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Close);
            _cancelButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Cancel);

            _saveButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Save);

            _deleteButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Delete);

            SetButtons();

            _availablePrefabs = PrefabStore.GetPrefabsOfType(prefabType)?
                .Select(prefab => prefab.GetComponent<TPrefab>())
                .Where(prefab => prefab.prefabType == prefabType)
                .ToHashSet();
        }

        private void Close(EWorkMode _)
        {
            if (!CanOpen) RemoveAndClose();
        }

        private void SetPrefabList(bool isOpen,
            IEnumerable<TPrefab> items = null,
            UnityAction onClose = null)
        {
            if (!isOpen)
            {
                _prefabList.SetActive(false);
                return;
            }

            if (items == null) return;

            _prefabList.Open(t.Get(Keys.AvailablePrefabs), items, prefab => SetPrefab(prefab.gameObject.name), onClose);
        }

        private void SetExistingList(bool isOpen,
            IEnumerable<TC> items = null,
            UnityAction onClose = null)
        {
            if (!isOpen)
            {
                _existingList.Close();
                return;
            }

            if (items == null) return;

            _existingList.Open(t.Get(Keys.ExistingPrefabs),
                items,
                configuration => Open(configuration as TC),
                true,
                onClose);
        }

        private void AssignComponents()
        {
            Transform bodyTransform = body.transform;
            Transform frame = bodyTransform.GetChild(0).GetChild(0);
            Content = frame.Find("ScrollingContent/Viewport/Content");
            
            Placeholder = bodyTransform.Find("Placeholder").gameObject;
            _prefabList = bodyTransform.Find("AvailablePrefabs").GetComponent<PrefabList>();
            _existingList = bodyTransform.Find("ExistingPrefabs").GetComponent<ConfigurationList>();
            _prefabTitle = frame.Find("Header/PrefabTitle").GetComponent<Title>();
            _prefabsFinderButton = frame.Find("Header/PrefabFinderButton").GetComponent<ImageButton>();
            _statusText = Content.Find("StatusText").GetComponent<TMP_Text>();
            Transform buttons = frame.Find("Buttons");
            _mainWindow = bodyTransform.Find("Background").gameObject;
            _cancelButton = buttons.Find("CancelButton").GetComponent<Button>();
            _cancelButton.SetTextColor(Colors.Warning);
            _saveButton = buttons.Find("SaveButton").GetComponent<Button>();
            _saveButton.SetTextColor(Colors.Positive);
            _deleteButton = buttons.Find("DeleteButton").GetComponent<Button>();
            _deleteButton.SetTextColor(Colors.Negative);
            _closeButton = buttons.Find("CloseButton").GetComponent<Button>();
        }
    }
}