using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Walls;
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
    public class WallEditorWindow : EditorWindowBase
    {
        [SerializeField] private PrefabList prefabList;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Title prefabTitle;
        [SerializeField] private LabeledSlider offsetSlider;
        [SerializeField] private GameObject placeholderWall;
        [SerializeField] private TMP_Text statusText;

        private MapBuilder MapBuilder => MapEditorManager.Instance.MapBuilder;
        private WallConfiguration _editedWallConfiguration;
        private WallConfiguration _originalConfiguration;
        private EWallType _editedWallType;
        private GameObject _physicalWall;
        private HashSet<WallPrefabBase> _availablePrefabs;
        private Cursor3D _cursor3D;

        private bool _isEditingExistingWall;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CloseWithChangeCheck);
            confirmButton.onClick.AddListener(SaveMapAndClose);
            deleteButton.onClick.AddListener(Delete);

            _cursor3D = FindObjectOfType<Cursor3D>();
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += Close;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= Close;
        }

        public void Open(WallConfiguration configuration)
        {
            string prefabListTitle = SetupWindow(configuration.WallType, true);

            _isEditingExistingWall = true;

            placeholderWall.transform.position = configuration.TransformData.Position;
            placeholderWall.transform.rotation = configuration.TransformData.Rotation;

            _editedWallType = configuration.WallType;
            _editedWallConfiguration = configuration;
            _originalConfiguration = new WallConfiguration(configuration);
            
            _cursor3D.ShowAt(configuration.TransformData.Position,
                new Vector3(0.15f, 1f, 1f),
                configuration.TransformData.Rotation);

            _physicalWall = MapBuilder.GetWallByConfiguration(configuration).GetComponentInChildren<MeshFilter>()?.gameObject;
            
            if (_physicalWall)
            {
                offsetSlider.SetActive(true);
                offsetSlider.Value = configuration.Offset;
                offsetSlider.slider.onValueChanged.RemoveAllListeners();
                offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
            }
            
            prefabTitle.SetActive(true);
            prefabTitle.SetTitle(configuration.PrefabName);
            
            prefabList.Open(prefabListTitle, _availablePrefabs!.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        public void Open(EWallType wallType, PositionRotation placeholderTransformData)
        {
            string prefabListTitle = SetupWindow(wallType, false);

            placeholderWall.transform.position = placeholderTransformData.Position;
            placeholderWall.transform.rotation = placeholderTransformData.Rotation;
            placeholderWall.transform.parent = null;
            placeholderWall.SetActive(true);

            if (_availablePrefabs == null || !_availablePrefabs.Any())
            {
                _editedWallType = EWallType.Invalid;
                SetStatusText(T.Get(Keys.NoPrefabsAvailable));
                return;
            }

            _editedWallType = wallType;

            SetStatusText(T.Get(Keys.SelectPrefab));

            prefabList.Open(prefabListTitle, _availablePrefabs.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        private string SetupWindow(EWallType wallType, bool deleteButtonActive)
        {
            SetActive(true);
            prefabList.SetActive(false);
            SetStatusText();
            _isEditingExistingWall = false;

            cancelButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Cancel);
            
            confirmButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Confirm);
            confirmButton.gameObject.SetActive(false);
            
            deleteButton.GetComponentInChildren<TMP_Text>().text = T.Get(Keys.Delete);
            deleteButton.gameObject.SetActive(deleteButtonActive);
            
            offsetSlider.SetLabel(T.Get(Keys.Offset));
            offsetSlider.SetActive(false);
            
            string prefabListTitle = T.Get(Keys.AvailablePrefabs);

            _availablePrefabs = PrefabStore.GetPrefabsOfType(EPrefabType.Wall)?
                .Select(prefab => prefab.GetComponent<WallPrefabBase>())
                .Where(prefab => prefab.GetWallType() == wallType)
                .ToHashSet();

            return prefabListTitle;
        }

        private void SetPrefab(string prefabName)
        {
            _isEditingExistingWall = false;
            
            if (_editedWallConfiguration != null)
            {
                MapBuilder.RemovePrefab(_editedWallConfiguration);
                _editedWallConfiguration = null;
            }

            SetStatusText();
            
            _editedWallConfiguration = new WallConfiguration
            {
                WallType = _editedWallType,
                PrefabName = _availablePrefabs.FirstOrDefault(prefab => prefab.name == prefabName)?.name,
                TransformData = new PositionRotation(placeholderWall.transform.position, placeholderWall.transform.rotation),
                WayPoints = new List<Vector3>(),
                Offset = 0f
            };

            if (!MapBuilder.BuildPrefab(_editedWallConfiguration))
            {
                SetStatusText(T.Get(Keys.ErrorBuildingPrefab));
                return;
            }

            prefabTitle.SetActive(true);
            prefabTitle.SetTitle(_editedWallConfiguration.PrefabName);

            _physicalWall = MapBuilder.GetWallByConfiguration(_editedWallConfiguration)?
                .GetComponentInChildren<MeshFilter>()?.gameObject;

            if (_physicalWall)
            {
                offsetSlider.SetActive(true);
                offsetSlider.Value = _editedWallConfiguration.Offset;
                offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
            }
            
            EditorEvents.TriggerOnMapEdited();
            deleteButton.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            placeholderWall.SetActive(false);
        }

        private void Delete()
        {
            WallConfiguration oldConfiguration = new (_editedWallConfiguration);
            _editedWallConfiguration = null;
            _physicalWall = null;
            prefabTitle.SetActive(false);
            
            Open(_editedWallType,
                new PositionRotation(oldConfiguration.TransformData.Position,
                    oldConfiguration.TransformData.Rotation));
            
            _cursor3D.Hide();
            
            MapBuilder.RemovePrefab(oldConfiguration);
        }

        private void Close(EWorkMode _) => CloseWithChangeCheck();

        public void CloseWithChangeCheck()
        {
            if (_isEditingExistingWall)
            {
                MapBuilder.RemovePrefab(_editedWallConfiguration);
                MapBuilder.BuildPrefab(_originalConfiguration);
            }
            
            if (!_isEditingExistingWall && _editedWallConfiguration != null)
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
            MapBuilder.RemovePrefab(_editedWallConfiguration);
            Close();
        }

        private void SaveMapAndClose()
        {
            MapBuilder.ReplacePrefabConfiguration(_editedWallConfiguration);
            MapEditorManager.Instance.SaveMap();
            Close();
        }

        private void Close()
        {
            _editedWallConfiguration = null;
            _editedWallType = EWallType.Invalid;

            placeholderWall.transform.position = Vector3.zero;
            placeholderWall.transform.parent = body.transform;
            placeholderWall.SetActive(false);

            prefabTitle.SetActive(false);
            _physicalWall = null;
            
            _cursor3D.Hide();
            
            EditorUIManager.Instance.IsAnyObjectEdited = false;
            EditorUIManager.Instance.WallGizmo.Reset();
            EditorMouseService.Instance.RefreshMousePosition();

            SetActive(false);
        }

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

        private void OnOffsetSliderValueChanged(float value)
        {
            confirmButton.gameObject.SetActive(true);
            EditorEvents.TriggerOnMapEdited();
            Vector3 newPosition = _physicalWall.transform.localPosition;
            newPosition.x = value;
            _editedWallConfiguration.Offset = value;
            _physicalWall.transform.localPosition = newPosition;
        }
    }
}