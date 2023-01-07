using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.EventsManagement;
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
using UnityEngine.UI;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public abstract class PrefabEditorBase<TC, TPrefab> : EditorWindowBase, IPrefabEditor
        where TC : PrefabConfiguration
        where TPrefab : PrefabBase
    {
        protected GameObject Placeholder;

        private PrefabList _prefabList;
        private Button _confirmButton;
        private Button _deleteButton;
        private Button _cancelButton;
        private Title _prefabTitle;
        private TMP_Text _statusText;

        protected MapBuilder MapBuilder => MapEditorManager.Instance.MapBuilder;
        protected TC EditedConfiguration;
        protected EPrefabType EditedPrefabType;
        protected GameObject PhysicalPrefabBody;
        protected GameObject PhysicalPrefab;
        
        private TC _originalConfiguration;
        private HashSet<TPrefab> _availablePrefabs;
        private Cursor3D _cursor3D;
        private bool _isEditingExistingPrefab;
        
        protected bool CanOpen => !body.activeSelf;

        private void Awake()
        {
            AssignComponents();

            _cancelButton.onClick.AddListener(CloseWithChangeCheck);
            _confirmButton.onClick.AddListener(SaveMapAndClose);
            _deleteButton.onClick.AddListener(Delete);

            _cursor3D = FindObjectOfType<Cursor3D>();
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

        protected virtual Vector3 Cursor3DScale => Vector3.one;

        public virtual void Open(TC configuration)
        {
            if (!CanOpen) return;

            if (configuration == null)
            {
                Close();
                return;
            }
            
            string prefabListTitle = SetupWindow(configuration.PrefabType, true);

            MoveCameraToPrefab(configuration.TransformData.Position);

            _isEditingExistingPrefab = true;

            EditedPrefabType = configuration.PrefabType;
            EditedConfiguration = configuration;
            _originalConfiguration = CloneConfiguration(configuration);

            _cursor3D.ShowAt(configuration.TransformData.Position,
                Cursor3DScale,
                configuration.TransformData.Rotation);

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(configuration.PrefabName);

            _prefabList.Open(prefabListTitle, _availablePrefabs!, SetPrefab);

            PhysicalPrefab = MapBuilder.GetPrefabByConfiguration(configuration);
            PhysicalPrefabBody = PhysicalPrefab.GetBody()?.gameObject;
            
            VisualizeOtherComponents();
        }

        public void Open(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            if (!CanOpen) return;

            EditorCameraService.Instance.MoveCameraToPrefab(placeholderTransformData.Position);
            
            string prefabListTitle = SetupWindow(prefabType, false);

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
            
            _cursor3D.ShowAt(placeholderTransformData.Position,
                Cursor3DScale,
                placeholderTransformData.Rotation);

            EditedPrefabType = prefabType;

            SetStatusText(t.Get(Keys.SelectPrefab));

            _prefabList.Open(prefabListTitle, _availablePrefabs, SetPrefab);
        }
        
        public virtual void CloseWithChangeCheck()
        {
            if (_isEditingExistingPrefab)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                MapBuilder.BuildPrefab(_originalConfiguration);
            }

            if (!_isEditingExistingPrefab && EditedConfiguration != null)
            {
                EditorUIManager.Instance.ConfirmationDialog.Open(t.Get(Keys.SaveEditedMapPrompt),
                    SaveMapAndClose,
                    RemoveAndClose,
                    t.Get(Keys.Save),
                    t.Get(Keys.DontSave));
                return;
            }

            Close();
        }

        protected virtual void SetPrefab(string prefabName)
        {
            SetStatusText();
            _isEditingExistingPrefab = false;

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

            PhysicalPrefab = MapBuilder.GetPrefabByConfiguration(EditedConfiguration);
            
            if (PhysicalPrefab)
            {
                PhysicalPrefabBody = PhysicalPrefab.GetBody()?.gameObject;
            }

            SetEdited();
            _deleteButton.gameObject.SetActive(true);
            Placeholder.SetActive(false);
            
            VisualizeOtherComponents();
        }
        
        protected void MoveCameraToPrefab(Vector3 targetPosition) => 
            EditorCameraService.Instance.MoveCameraToPrefab(targetPosition);

        protected void SetEdited()
        {
            _confirmButton.gameObject.SetActive(true);
            EditorEvents.TriggerOnMapEdited();
        }

        protected virtual void Delete()
        {
            MapBuilder.RemovePrefab(EditedConfiguration);

            MapEditorManager.Instance.SaveMap();
            
            Close();
        }

        protected virtual void RemoveAndClose()
        {
            MapBuilder.RemovePrefab(EditedConfiguration);
            Close();
        }

        protected virtual void SaveMapAndClose()
        {
            MapBuilder.AddReplacePrefabConfiguration(EditedConfiguration);
            MapEditorManager.Instance.SaveMap();
            Close();
        }

        protected void Close()
        {
            EditedConfiguration = null;
            _originalConfiguration = null;
            _isEditingExistingPrefab = false;
            EditedPrefabType = EPrefabType.Invalid;

            Placeholder.transform.position = Vector3.zero;
            Placeholder.transform.parent = body.transform;
            Placeholder.SetActive(false);

            _prefabTitle.SetActive(false);
            PhysicalPrefabBody = null;
            PhysicalPrefab = null;

            _cursor3D.Hide();

            EditorUIManager.Instance.isAnyObjectEdited = false;
            EditorUIManager.Instance.WallGizmo.Reset();
            EditorMouseService.Instance.RefreshMousePosition();

            SetActive(false);
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
        
        private string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            InitializeOtherComponents();
            
            SetActive(true);
            _prefabList.SetActive(false);
            SetStatusText();
            _isEditingExistingPrefab = false;

            _cancelButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Cancel);

            _confirmButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Confirm);
            _confirmButton.gameObject.SetActive(false);

            _deleteButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Delete);
            _deleteButton.gameObject.SetActive(deleteButtonActive);

            string prefabListTitle = t.Get(Keys.AvailablePrefabs);

            _availablePrefabs = PrefabStore.GetPrefabsOfType(prefabType)?
                .Select(prefab => prefab.GetComponent<TPrefab>())
                .Where(prefab => prefab.prefabType == prefabType)
                .ToHashSet();

            return prefabListTitle;
        }
        
        private void Close(EWorkMode _)
        {
            if (!CanOpen) CloseWithChangeCheck();
        }
        
        private void AssignComponents()
        {
            Placeholder = body.transform.Find("Placeholder").gameObject;
            _prefabList = body.transform.Find("PrefabList").GetComponent<PrefabList>();
            Transform frame = body.transform.GetChild(0).GetChild(0);
            _prefabTitle = frame.Find("PrefabTitle").GetComponent<Title>();
            _statusText = frame.Find("StatusText").GetComponent<TMP_Text>();
            Transform buttons = frame.Find("Buttons");
            _cancelButton = buttons.Find("CancelButton").GetComponent<Button>();
            _confirmButton = buttons.Find("ConfirmButton").GetComponent<Button>();
            _deleteButton = buttons.Find("DeleteButton").GetComponent<Button>();
        }
    }
}