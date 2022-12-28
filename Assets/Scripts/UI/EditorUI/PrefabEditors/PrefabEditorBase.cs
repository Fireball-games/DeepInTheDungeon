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
        private TC _originalConfiguration;
        protected EPrefabType EditedPrefabType;
        protected GameObject PhysicalPrefabBody;
        protected GameObject PhysicalPrefab;
        protected HashSet<TPrefab> AvailablePrefabs;
        private Cursor3D Cursor3D;

        private bool _isEditingExistingPrefab;

        private void Awake()
        {
            AssignComponents();

            _cancelButton.onClick.AddListener(CloseWithChangeCheck);
            _confirmButton.onClick.AddListener(SaveMapAndClose);
            _deleteButton.onClick.AddListener(Delete);

            Cursor3D = FindObjectOfType<Cursor3D>();
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
        protected abstract TC CopyConfiguration(TC sourceConfiguration);

        protected virtual Vector3 Cursor3DScale => Vector3.one;

        public virtual void Open(TC configuration)
        {
            string prefabListTitle = SetupWindow(configuration.PrefabType, true);
            
            EditorCameraService.Instance.MoveCameraToPrefab(configuration.TransformData.Position);

            _isEditingExistingPrefab = true;

            EditedPrefabType = configuration.PrefabType;
            EditedConfiguration = configuration;
            _originalConfiguration = CopyConfiguration(configuration);

            Cursor3D.ShowAt(configuration.TransformData.Position,
                Cursor3DScale,
                configuration.TransformData.Rotation);

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(configuration.PrefabName);

            _prefabList.Open(prefabListTitle, AvailablePrefabs!.Select(prefab => prefab.gameObject.name), SetPrefab);

            PhysicalPrefab = MapBuilder.GetPrefabByConfiguration(configuration);
            PhysicalPrefabBody = PhysicalPrefab.GetBody()?.gameObject;
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

            if (AvailablePrefabs == null || !AvailablePrefabs.Any())
            {
                EditedPrefabType = EPrefabType.Invalid;
                SetStatusText(t.Get(Keys.NoPrefabsAvailable));
                return;
            }
            
            Cursor3D.ShowAt(placeholderTransformData.Position,
                Cursor3DScale,
                placeholderTransformData.Rotation);

            EditedPrefabType = prefabType;

            SetStatusText(t.Get(Keys.SelectPrefab));

            _prefabList.Open(prefabListTitle, AvailablePrefabs.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        protected virtual string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
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

            AvailablePrefabs = PrefabStore.GetPrefabsOfType(prefabType)?
                .Select(prefab => prefab.GetComponent<TPrefab>())
                .Where(prefab => prefab.prefabType == prefabType)
                .ToHashSet();

            return prefabListTitle;
        }

        protected virtual void SetPrefab(string prefabName)
        {
            _isEditingExistingPrefab = false;

            if (EditedConfiguration != null)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                EditedConfiguration = null;
            }

            SetStatusText();

            EditedConfiguration = GetNewConfiguration(prefabName);

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
        }

        protected void SetEdited()
        {
            _confirmButton.gameObject.SetActive(true);
            EditorEvents.TriggerOnMapEdited();
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

        protected virtual void Delete()
        {
            MapBuilder.RemovePrefab(EditedConfiguration);

            MapEditorManager.Instance.SaveMap();
            
            Close();
        }

        private void Close(EWorkMode _)
        {
            if (!CanOpen) CloseWithChangeCheck();
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

            Cursor3D.Hide();

            EditorUIManager.Instance.IsAnyObjectEdited = false;
            EditorUIManager.Instance.WallGizmo.Reset();
            EditorMouseService.Instance.RefreshMousePosition();

            SetActive(false);
        }

        protected bool CanOpen => !body.activeSelf;

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
    }
}