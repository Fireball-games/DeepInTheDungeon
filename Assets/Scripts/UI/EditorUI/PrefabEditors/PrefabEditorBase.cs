using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
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

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public abstract class PrefabEditorBase<TC, TPrefab> : EditorWindowBase, IPrefabEditor
        where TC : PrefabConfiguration
        where TPrefab : PrefabBase
    {
        protected GameObject Placeholder;
        protected Button ConfirmButton;

        private PrefabList _prefabList;
        private Button _cancelButton;
        private Button _deleteButton;
        private Title _prefabTitle;
        private TMP_Text _statusText;

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
            AssignComponents();

            _cancelButton.onClick.AddListener(CloseWithChangeCheck);
            ConfirmButton.onClick.AddListener(SaveMapAndClose);
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

            _isEditingExistingPrefab = true;

            Placeholder.transform.position = configuration.TransformData.Position;
            Placeholder.transform.rotation = configuration.TransformData.Rotation;

            EditedPrefabType = configuration.PrefabType;
            EditedConfiguration = configuration;
            _originalConfiguration = CopyConfiguration(configuration);

            Cursor3D.ShowAt(configuration.TransformData.Position,
                Cursor3DScale,
                configuration.TransformData.Rotation);

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(configuration.PrefabName);

            _prefabList.Open(prefabListTitle, AvailablePrefabs!.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        public void Open(EPrefabType prefabType, PositionRotation placeholderTransformData)
        {
            if (!CanOpen) return;

            string prefabListTitle = SetupWindow(prefabType, false);

            Placeholder.transform.position = placeholderTransformData.Position;
            Placeholder.transform.rotation = placeholderTransformData.Rotation;
            Placeholder.transform.parent = null;
            Placeholder.SetActive(true);

            if (AvailablePrefabs == null || !AvailablePrefabs.Any())
            {
                EditedPrefabType = EPrefabType.Invalid;
                SetStatusText(T.Get(Keys.NoPrefabsAvailable));
                return;
            }

            EditedPrefabType = prefabType;

            SetStatusText(T.Get(Keys.SelectPrefab));

            _prefabList.Open(prefabListTitle, AvailablePrefabs.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        protected virtual string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            SetActive(true);
            _prefabList.SetActive(false);
            SetStatusText();
            _isEditingExistingPrefab = false;

            _cancelButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Cancel);

            ConfirmButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Confirm);
            ConfirmButton.gameObject.SetActive(false);

            _deleteButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Delete);
            _deleteButton.gameObject.SetActive(deleteButtonActive);

            string prefabListTitle = T.Get(Keys.AvailablePrefabs);

            // IEnumerable<GameObject> storePrefabs = PrefabStore.GetPrefabsOfType(prefabType);
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
                SetStatusText(T.Get(Keys.ErrorBuildingPrefab));
                return;
            }

            _prefabTitle.SetActive(true);
            _prefabTitle.SetTitle(EditedConfiguration.PrefabName);

            PhysicalPrefab = MapBuilder.GetPrefabByConfiguration(EditedConfiguration)?
                .GetComponentInChildren<MeshFilter>()?.gameObject;

            EditorEvents.TriggerOnMapEdited();
            _deleteButton.gameObject.SetActive(true);
            ConfirmButton.gameObject.SetActive(true);
            Placeholder.SetActive(false);
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
            ConfirmButton = buttons.Find("ConfirmButton").GetComponent<Button>();
            _deleteButton = buttons.Find("DeleteButton").GetComponent<Button>();
        }

        private void Delete()
        {
            TC oldConfiguration = CopyConfiguration(EditedConfiguration);
            EditedConfiguration = null;
            PhysicalPrefab = null;
            _prefabTitle.SetActive(false);

            Cursor3D.Hide();

            MapBuilder.RemovePrefab(EditedConfiguration);
            MapBuilder.RemovePrefab(oldConfiguration);

            Close();
        }

        private void Close(EWorkMode _)
        {
            if (!CanOpen) CloseWithChangeCheck();
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

            Placeholder.transform.position = Vector3.zero;
            Placeholder.transform.parent = body.transform;
            Placeholder.SetActive(false);

            _prefabTitle.SetActive(false);
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