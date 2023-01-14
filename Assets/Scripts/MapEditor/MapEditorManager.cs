using System;
using System.Collections.Generic;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>>;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor
{
    public class MapEditorManager : SingletonNotPersisting<MapEditorManager>
    {
        public const int MinRows = 5;
        public const int MaxRows = 30;
        public const int MinColumns = 5;
        public const int MaxColumns = 30;
        public const int MinFloors = 3;
        public const int MaxFloors = 12;
        
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private PlayerIconController playerIcon;

        public ELevel WorkLevel { get; private set; }
        public EWorkMode WorkMode { get; private set; }
        public bool MapIsPresented { get; private set; }
        public bool MapIsChanged { get; set; }
        public bool MapIsSaved { get; set; } = true;
        public bool MapIsBeingBuilt { get; private set; }
        public LayoutType EditedLayout { get; private set; }
        public MapBuilder MapBuilder { get; private set; }
        public int CurrentFloor { get; private set; }
        public Dictionary<int, bool> FloorVisibilityMap { get; private set; }
        public ETriggerEditMode TriggerEditMode { get; private set; } = ETriggerEditMode.AddTrigger;

        private bool _dontChangeCameraAfterLayoutIsBuild;

        protected override void Awake()
        {
            base.Awake();

            FloorVisibilityMap = new Dictionary<int, bool>(); 
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);

            MapBuilder = GameManager.Instance.MapBuilder;
        }

        private void OnEnable()
        {
            MapBuilder.OnLayoutBuilt += OnLayoutBuilt;
            EditorEvents.OnMapEditedStatusChanged += MapEditedStatusChanged;
        }

        private void OnDisable()
        {
            MapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
            EditorEvents.OnMapEditedStatusChanged -= MapEditedStatusChanged;
        }

        public void OrderMapConstruction(MapDescription map,
            bool markMapAsSaved = false,
            bool mapIsPresented = false,
            bool useStartPosition = true,
            ELevel floorsCountChange = ELevel.Equal)
        {
            if (MapIsBeingBuilt) return;
            
            MapIsBeingBuilt = true;
            MapIsPresented = mapIsPresented;
            MapIsSaved = markMapAsSaved;
            
            EditedLayout = MapBuildService.ConvertToLayoutType(map.Layout);
            
            if (useStartPosition)
            {
                CurrentFloor = map.StartGridPosition.x;
            }

            if (floorsCountChange == ELevel.Upper)
            {
                CurrentFloor += 1;
            }
            
            _dontChangeCameraAfterLayoutIsBuild = false;
            
            RefreshFloorVisibilityMap();

            GameManager.Instance.SetCurrentMap(map);

            MapBuilder.BuildMap(map);

            EditorEvents.TriggerOnNewMapStartedCreation();
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            WorkMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(WorkMode);
        }

        public void SetWorkingLevel(ELevel newLevel)
        {
            WorkLevel = newLevel;
            EditorEvents.TriggerOnWorkingLevelChanged(WorkLevel);
        }
        
        public void SetTriggerEditMode(ETriggerEditMode newTriggerEditMode)
        {
            TriggerEditMode = newTriggerEditMode;
            // Dont want trigger separate event for TriggerEditMode just yet, wall gizmo reacts on work mode change, might change.
            EditorEvents.TriggerOnWorkModeChanged(EWorkMode.Triggers);
        }

        public void GoToMainMenu()
        {
            if (!MapIsSaved)
            {
                EditorUIManager.Instance.ConfirmationDialog.Open(
                    t.Get(Keys.SaveEditedMapPrompt),
                    GoToMainScreenWithSave,
                    LoadMainSceneClear,
                    t.Get(Keys.Save),
                    t.Get(Keys.DontSave));
                
                return;
            }
            
            LoadMainSceneClear();
        }
        
        public void PlayMap()
        {
            if (!MapIsPresented)
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(t.Get(Keys.NoMapToPlayLoaded), StatusBar.EMessageType.Negative);
                return;
            }

            MapDescription currentMap = GameManager.Instance.CurrentMap;
            
            SaveMap();

            GameManager.Instance.IsPlayingFromEditor = true;
            SceneLoader.Instance.LoadScene(currentMap.SceneName);
        }
        
        public void SaveMap()
        {
            MapDescription currentMap = GameManager.Instance.CurrentMap;
            
            string mapName = currentMap.MapName;

            string mapDirectoryPath = FileOperationsHelper.MapDirectoryPath;
            
            mapDirectoryPath.CreateDirectoryIfNotExists();

            try
            {
                ES3.Save(mapName, currentMap, FileOperationsHelper.GetSavePath(mapName));
            }
            catch (Exception e)
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(t.Get(Keys.SaveFailed), StatusBar.EMessageType.Negative);
                Logger.Log($"Saving file failed: {e.Message}");
                return;
            }
            
            EditorUIManager.Instance.StatusBar.RegisterMessage(t.Get(Keys.MapSaved), StatusBar.EMessageType.Positive);

            MapIsChanged = false;
            MapIsSaved = true;
        }
        
        public void SetFloor(int newFloor)
        {
            if (CurrentFloor == newFloor) return;
            
            CurrentFloor = newFloor;
            
            RefreshFloorVisibilityMap();
            
            MapBuildService.SetMapFloorsVisibility(FloorVisibilityMap);

            MapBuilder.SetPrefabsVisibility(FloorVisibilityMap);
            
            EditorEvents.TriggerOnFloorChanged(CurrentFloor);
        }

        private void GoToMainScreenWithSave()
        {
            SaveMap();
            LoadMainSceneClear();
        }

        private void LoadMainSceneClear()
        {
            EditorMouseService.Instance.ResetCursor();
            MapBuilder.DemolishMap();
            GameManager.Instance.SetCurrentMap(null);
            GameManager.Instance.IsPlayingFromEditor = false;
            SceneLoader.Instance.LoadMainScene();
        }

        private void OnLayoutBuilt()
        {
            Vector3 startPosition = GameManager.Instance.CurrentMap.StartGridPosition;
            
            if (!MapIsPresented && !_dontChangeCameraAfterLayoutIsBuild)
            {
                EditorCameraService.Instance.MoveCameraTo(startPosition.y, cameraHeight, startPosition.z);
            }
            
            MapIsBeingBuilt = false;
            MapIsPresented = true;

            SetWorkMode(EWorkMode.Build);

            MapDescription map = GameManager.Instance.CurrentMap;
            
            playerIcon.SetPositionByGrid(map.StartGridPosition);
            playerIcon.SetArrowRotation(map.PlayerRotation);
            playerIcon.SetActive(true);

            EditorCameraService.Instance.ResetCamera();
        }
        
        private void MapEditedStatusChanged(bool isEdited)
        {
            MapIsChanged = isEdited;
            MapIsSaved = false;
        }

        private void RefreshFloorVisibilityMap()
        {
            FloorVisibilityMap.Clear();
            
            for (int floor = 1; floor <= EditedLayout.Count - 2; floor++)
            {
                FloorVisibilityMap.Add(floor, floor >= CurrentFloor);
            }
        }
    }
}