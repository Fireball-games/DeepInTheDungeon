using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.UI.Components.MessageBar;
using static Scripts.Helpers.PlayerPrefsHelper;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MonoBases.DialogBase;
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
        [SerializeField] private EditorStartIndicator playerIcon;

        private GameManager GameManager => GameManager.Instance;

        public ELevel WorkLevel { get; private set; }
        public EWorkMode WorkMode { get; private set; }
        public EEditMode EditMode { get; private set; }
        public bool MapIsPresented { get; private set; }
        public bool MapIsChanged { get; private set; }
        public bool PrefabIsEdited { get; private set; }
        public bool MapIsBeingBuilt { get; private set; }
        public LayoutType EditedLayout { get; private set; }
        public MapBuilder MapBuilder
        {
            get
            {
                if (!GameManager || !GameManager.MapBuilder)
                    return null;
                
                return GameManager.MapBuilder;
            }
        }

        public int CurrentFloor { get; private set; }
        public Dictionary<int, bool> FloorVisibilityMap { get; private set; }

        private static EditorUIManager EditorUIManager => EditorUIManager.Instance;
        private MapDescription _originalMap;

        private bool _mapIsSaved = true;
        private bool _dontChangeCameraAfterLayoutIsBuild;

        protected override void Awake()
        {
            base.Awake();

            FloorVisibilityMap = new Dictionary<int, bool>();
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);
        }

        private void OnEnable()
        {
            MapBuilder.OnLayoutBuilt.AddListener(OnLayoutBuilt);
            EditorEvents.OnMapLayoutChanged += OnMapLayoutChanged;
            EditorEvents.OnPrefabEdited += OnPrefabEdited;
        }

        private void OnDisable()
        {
            if (MapBuilder)
            {
                MapBuilder.OnLayoutBuilt.RemoveListener(OnLayoutBuilt);
            }

            EditorEvents.OnMapLayoutChanged -= OnMapLayoutChanged;
            EditorEvents.OnPrefabEdited -= OnPrefabEdited;
        }

        public void OrderMapConstruction(MapDescription map,
            bool markMapAsSaved = false,
            bool mapIsPresented = false,
            bool useStartPosition = true,
            ELevel floorsCountChange = ELevel.Equal)
        {
            if (map == null)
            {
                Logger.LogError("Map is null, can't build the map, rebuilding original map.");
                map = GameManager.CurrentMap;
            }
            
            if (MapIsBeingBuilt) return;

            MapIsBeingBuilt = true;
            MapIsPresented = mapIsPresented;
            _mapIsSaved = markMapAsSaved;
            MapIsChanged = false;

            // Only when build is marked as saved, we can set original map, because we can assume then that map same as in a Campaign 
            // and on rebuild we don't want to save changes automatically.
            if (markMapAsSaved)
                _originalMap = map.ClonedCopy();
            
            EditedLayout = MapBuildService.ConvertToLayoutType(map.Layout);

            if (useStartPosition)
            {
                CurrentFloor = map.EditorStartPosition.x;
            }

            if (floorsCountChange == ELevel.Upper)
            {
                CurrentFloor += 1;
            }

            _dontChangeCameraAfterLayoutIsBuild = false;

            RefreshFloorVisibilityMap();

            GameManager.SetCurrentMap(map);

            MapBuilder.BuildMap(map);

            LastEditedMap = new[] {GameManager.CurrentCampaign.CampaignName, map.MapName};

            EditorEvents.TriggerOnNewMapStartedCreation();
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            if (newWorkMode == WorkMode) return;
            
            WorkMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(WorkMode);
        }

        public void SetWorkingLevel(ELevel newLevel)
        {
            if (newLevel == WorkLevel) return;
            
            WorkLevel = newLevel;
            EditorEvents.TriggerOnWorkingLevelChanged(WorkLevel);
        }
        
        public void SetEditMode(EEditMode newEditMode)
        {
            if (newEditMode == EditMode) return;
            
            EditMode = newEditMode;
            EditorEvents.TriggerOnEditModeChanged(EditMode);
        }

        public void LoadMainSceneClear()
        {
            EditorMouseService.Instance.ResetCursor();
            EditedLayout = null;
            MapBuilder.DemolishMap();
            GameManager.SetCurrentMap(null);
            GameManager.IsPlayingFromEditor = false;
            GameManager.StartMainScene();
        }

        public async void PlayMap()
        {
            if (await CheckToSaveMapChanges() is EConfirmResult.Cancel)
            {
                GameManager.SetCurrentMap(_originalMap);
            }

            if (!MapIsPresented)
            {
                EditorUIManager.MessageBar.Set(t.Get(Keys.NoMapToPlayLoaded), EMessageType.Negative, automaticDismissDelay: 3f);
                return;
            }

            GameManager.IsPlayingFromEditor = true;
            GameManager.LoadLastEditedMap();
        }
        
        public async void PlayFromEntryPoint(EntryPoint entryPoint)
        {
            if (await CheckToSaveMapChanges() is EConfirmResult.Cancel)
            {
                GameManager.SetCurrentMap(_originalMap);
            }

            GameManager.IsPlayingFromEditor = true;
            GameManager.LoadLastEditedMap(entryPoint);
        }

        /// <summary>
        /// Makes check if map is changed and if so, asks user if he wants to save changes. 
        /// </summary>
        /// <returns>Ok if is nothing to save or its saved, if user don't want to save, returns Cancel</returns>
        public async Task<EConfirmResult> CheckToSaveMapChanges()
        {
            EConfirmResult result = EConfirmResult.Ok;
            
            if (MapIsChanged || PrefabIsEdited || !_mapIsSaved) {
                result = await EditorUIManager.ConfirmationDialog.Show(
                    t.Get(Keys.SaveEditedMapPrompt),
                    t.Get(Keys.SaveMap),
                    t.Get(Keys.DontSave)
                );

                if (result is EConfirmResult.Ok)
                {
                    SaveMap();
                }
            }
            
            return result;
        }

        public void SaveMap(bool isSilent = false)
        {
            Campaign currentCampaign = GameManager.CurrentCampaign;

            string campaignDirectoryPath = FileOperationsHelper.CampaignsLocalDirectoryPath;

            campaignDirectoryPath.CreateDirectoryIfNotExists();
            GameManager.CurrentMap.EntryPoints = EntryPointService.ConvertEntryPointConfigurationsToEntryPoints();
            GameManager.CurrentMap.MapObjects = MapBuilder.CollectMapObjects();
            currentCampaign.ReplaceMap(GameManager.CurrentMap);
            _originalMap = GameManager.CurrentMap.ClonedCopy();
            
            FileOperationsHelper.SaveCampaign(currentCampaign,
                () => EditorUIManager.MessageBar.Set(t.Get(Keys.SaveFailed), EMessageType.Negative, automaticDismissDelay: 3f));

            MapIsChanged = false;
            _mapIsSaved = true;
            PrefabIsEdited = false;
            
            if (!isSilent)
            {
                EditorUIManager.MessageBar.Set(t.Get(Keys.MapSaved), EMessageType.Positive, automaticDismissDelay: 1f);
                EditorEvents.TriggerOnMapSaved();
            }
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
        
        public void SetCurrentMapToOriginalMap() => GameManager.SetCurrentMap(_originalMap);

        private void OnLayoutBuilt()
        {
            Vector3 startPosition = GameManager.CurrentMap.EditorStartPosition;

            if (!MapIsPresented && !_dontChangeCameraAfterLayoutIsBuild)
            {
                EditorCameraService.Instance.MoveCameraTo(startPosition.y, cameraHeight, startPosition.z);
            }

            MapIsBeingBuilt = false;
            MapIsPresented = true;

            SetWorkMode(EWorkMode.Build);

            MapDescription map = GameManager.CurrentMap;

            playerIcon.SetPositionByGrid(map.EditorStartPosition);
            playerIcon.SetArrowRotation(map.EditorPlayerStartRotation);
            playerIcon.SetActive(true);

            EditorCameraService.Instance.ResetCamera();
            
            EditorEvents.TriggerOnMapBuilt();
        }

        private void OnPrefabEdited(bool isEdited)
        {
            PrefabIsEdited = isEdited;
            
            if (!isEdited) return;
            
            MapIsChanged = true;
            _mapIsSaved = false;
        }

        private void OnMapLayoutChanged()
        {
            MapIsChanged = true;
            _mapIsSaved = false;
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