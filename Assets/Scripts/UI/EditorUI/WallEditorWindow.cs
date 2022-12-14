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
        [SerializeField] private Button CancelButton;
        [SerializeField] private GameObject placeholderWall;
        [SerializeField] private TMP_Text statusText;

        private MapBuilder MapBuilder => MapEditorManager.Instance.MapBuilder;
        private WallConfiguration _editedWallConfiguration;
        private EWallType _editedWallType;
        private HashSet<WallPrefabBase> _availablePrefabs;

        private void Awake()
        {
            CancelButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += Close;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= Close;
        }

        public void Open(EWallType wallType, PositionRotation placeholderTransformData)
        {
            SetActive(true);
            prefabList.SetActive(false);
            SetStatusText();

            CancelButton.GetComponentInChildren<TMP_Text>().text = T.Get(LocalizationKeys.Cancel);
            string title = T.Get(LocalizationKeys.AvailablePrefabs);

            placeholderWall.transform.position = placeholderTransformData.Position;
            placeholderWall.transform.rotation = placeholderTransformData.Rotation;
            placeholderWall.transform.parent = null;
            placeholderWall.SetActive(true);

            _availablePrefabs = PrefabStore.GetPrefabsOfType(EPrefabType.Wall)?
                .Select(prefab => prefab.GetComponent<WallPrefabBase>())
                .Where(prefab => prefab.GetWallType() == wallType)
                .ToHashSet();

            if (_availablePrefabs == null || !_availablePrefabs.Any())
            {
                _editedWallType = EWallType.Invalid;
                SetStatusText(T.Get(LocalizationKeys.NoPrefabsAvailable));
                return;
            }

            _editedWallType = wallType;
            
            SetStatusText(T.Get(LocalizationKeys.SelectPrefab));

            prefabList.Open(title, _availablePrefabs.Select(prefab => prefab.gameObject.name), SetPrefab);
        }

        private void SetPrefab(string prefabName)
        {
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
            
            if(!MapBuilder.BuildPrefab(_editedWallConfiguration))
            {
                SetStatusText(T.Get(LocalizationKeys.ErrorBuildingPrefab));
                return;
            }

            placeholderWall.SetActive(false);
        }

        private void Close(EWorkMode _) => Close();
        
        public void Close()
        {
            placeholderWall.transform.position = Vector3.zero;
            placeholderWall.transform.parent = body.transform;
            placeholderWall.SetActive(false);
            EditorUIManager.Instance.IsAnyObjectEdited = false;
            
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
    }
}