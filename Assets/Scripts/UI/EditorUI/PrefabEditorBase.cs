using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.Walls;
using Scripts.Building.Walls.Configurations;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public abstract class PrefabEditorBase <TC, TPrefab> : EditorWindowBase where TC : PrefabConfiguration, new() where TPrefab : PrefabBase
    {
        [SerializeField] protected PrefabList prefabList;
        [SerializeField] protected Button cancelButton;
        [SerializeField] protected Button confirmButton;
        [SerializeField] protected Button deleteButton;
        [SerializeField] protected Title prefabTitle;
        [SerializeField] protected GameObject placeholder;
        [SerializeField] private TMP_Text statusText;

        protected MapBuilder MapBuilder => MapEditorManager.Instance.MapBuilder;
        protected TC EditedConfiguration;
        private TC _originalConfiguration;
        protected EPrefabType EditedPrefabType;
        protected GameObject PhysicalPrefab;
        protected HashSet<TPrefab> AvailablePrefabs;
        private Cursor3D Cursor3D;

        private bool _isEditingExistingPrefab;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CloseWithChangeCheck);
            confirmButton.onClick.AddListener(SaveMapAndClose);
            deleteButton.onClick.AddListener(Delete);

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

        protected abstract Vector3 GetCursor3DScale();

        public virtual void Open(TC configuration)
        {

            string prefabListTitle = SetupWindow(configuration.PrefabType, true);

            _isEditingExistingPrefab = true;

            placeholder.transform.position = configuration.TransformData.Position;
            placeholder.transform.rotation = configuration.TransformData.Rotation;

            EditedPrefabType = configuration.PrefabType;
            EditedConfiguration = configuration;
            _originalConfiguration = CopyConfiguration(configuration);
            
            Cursor3D.ShowAt(configuration.TransformData.Position,
                GetCursor3DScale(),
                configuration.TransformData.Rotation);

            prefabTitle.SetActive(true);
            prefabTitle.SetTitle(configuration.PrefabName);
            
            prefabList.Open(prefabListTitle, AvailablePrefabs!.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        public void Open(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            if (!CanOpen) return;
            
            string prefabListTitle = SetupWindow(prefabType, false);

            placeholder.transform.position = placeholderTransformData.Position;
            placeholder.transform.rotation = placeholderTransformData.Rotation;
            placeholder.transform.parent = null;
            placeholder.SetActive(true);

            if (AvailablePrefabs == null || !AvailablePrefabs.Any())
            {
                EditedPrefabType = EPrefabType.Invalid;
                SetStatusText(T.Get(Keys.NoPrefabsAvailable));
                return;
            }

            EditedPrefabType = prefabType;

            SetStatusText(T.Get(Keys.SelectPrefab));

            prefabList.Open(prefabListTitle, AvailablePrefabs.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        protected virtual string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            SetActive(true);
            prefabList.SetActive(false);
            SetStatusText();
            _isEditingExistingPrefab = false;

            cancelButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Cancel);
            
            confirmButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Confirm);
            confirmButton.gameObject.SetActive(false);
            
            deleteButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Delete);
            deleteButton.gameObject.SetActive(deleteButtonActive);

            string prefabListTitle = T.Get(Keys.AvailablePrefabs);

            AvailablePrefabs = PrefabStore.GetPrefabsOfType(EPrefabType.Wall)?
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
                SetStatusText(T.Get(Keys.ErrorBuildingPrefab));
                return;
            }

            prefabTitle.SetActive(true);
            prefabTitle.SetTitle(EditedConfiguration.PrefabName);

            PhysicalPrefab = MapBuilder.GetWallByConfiguration(EditedConfiguration)?
                .GetComponentInChildren<MeshFilter>()?.gameObject;

            EditorEvents.TriggerOnMapEdited();
            deleteButton.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            placeholder.SetActive(false);
        }

        private void Delete()
        {
            TC oldConfiguration = CopyConfiguration(EditedConfiguration);
            EditedConfiguration = null;
            PhysicalPrefab = null;
            prefabTitle.SetActive(false);
            
            Open(EditedPrefabType,
                new PositionRotation(oldConfiguration.TransformData.Position,
                    oldConfiguration.TransformData.Rotation));
            
            Cursor3D.Hide();
            
            MapBuilder.RemovePrefab(EditedConfiguration);
            MapBuilder.RemovePrefab(oldConfiguration);
            
            Close();
        }

        private void Close(EWorkMode _)
        {
            if(!CanOpen) CloseWithChangeCheck();
        }

        public void CloseWithChangeCheck()
        {
            if (_isEditingExistingPrefab)
            {
                MapBuilder.RemovePrefab(EditedConfiguration);
                MapBuilder.BuildPrefab(_originalConfiguration);
            }
            
            if (!_isEditingExistingPrefab && EditedConfiguration != null)
            {
                EditorUIManager.Instance.ConfirmationDialog.Open(T.Get(Keys.SaveEditedMapPrompt),
                    SaveMapAndClose,
                    RemoveAndClose);
                return;
            }

            Close();
        }

        private void RemoveAndClose()
        {
            MapBuilder.RemovePrefab(EditedConfiguration);
            Close();
        }

        private void SaveMapAndClose()
        {
            MapBuilder.ReplacePrefabConfiguration(EditedConfiguration);
            MapEditorManager.Instance.SaveMap();
            Close();
        }

        protected void Close()
        {
            EditedConfiguration = null;
            _originalConfiguration = null;
            _isEditingExistingPrefab = false;
            EditedPrefabType = EPrefabType.Invalid;

            placeholder.transform.position = Vector3.zero;
            placeholder.transform.parent = body.transform;
            placeholder.SetActive(false);

            prefabTitle.SetActive(false);
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
            statusText.text = text ?? "";

            if (string.IsNullOrEmpty(text))
            {
                statusText.gameObject.SetActive(false);
                return;
            }

            statusText.gameObject.SetActive(true);
        }
    }
}