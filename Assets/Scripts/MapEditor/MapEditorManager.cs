using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.EditorUI;
using UnityEngine;
using static MessageBar;
using static Scripts.Helpers.PlayerPrefsHelper;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MonoBases.DialogBase;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>>;

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

        private GameManager GameManager => GameManager.Instance;

        public ELevel WorkLevel { get; private set; }
        public EWorkMode WorkMode { get; private set; }
        public bool MapIsPresented { get; private set; }
        public bool MapIsChanged { get; private set; }
        public bool PrefabIsEdited { get; private set; }
        public bool MapIsBeingBuilt { get; private set; }
        public LayoutType EditedLayout { get; private set; }
        public MapBuilder MapBuilder => GameManager.MapBuilder;
        public int CurrentFloor { get; private set; }
        public Dictionary<int, bool> FloorVisibilityMap { get; private set; }

        private static EditorUIManager EditorUIManager => EditorUIManager.Instance;

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
            if (MapIsBeingBuilt) return;

            MapIsBeingBuilt = true;
            MapIsPresented = mapIsPresented;
            _mapIsSaved = markMapAsSaved;
            MapIsChanged = false;

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

            GameManager.SetCurrentMap(map);

            MapBuilder.BuildMap(map);

            LastEditedMap = new[] {GameManager.CurrentCampaign.CampaignName, map.MapName};

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

        public void LoadMainSceneClear()
        {
            EditorMouseService.Instance.ResetCursor();
            EditedLayout = null;
            MapBuilder.DemolishMap();
            GameManager.SetCurrentMap(null);
            GameManager.IsPlayingFromEditor = false;
            SceneLoader.Instance.LoadMainScene();
        }

        public void PlayMap()
        {
            if (!MapIsPresented)
            {
                EditorUIManager.MessageBar.Set(t.Get(Keys.NoMapToPlayLoaded), EMessageType.Negative, automaticDismissDelay: 3f);
                return;
            }

            MapDescription currentMap = GameManager.CurrentMap;

            SaveMap();

            GameManager.IsPlayingFromEditor = true;
            SceneLoader.Instance.LoadScene(currentMap.SceneName);
        }

        public async Task CheckToSaveMapChanges()
        {
            if ((MapIsChanged || PrefabIsEdited || !_mapIsSaved) && await EditorUIManager.ConfirmationDialog.Show(
                    t.Get(Keys.SaveEditedMapPrompt),
                    t.Get(Keys.SaveMap),
                    t.Get(Keys.DontSave)
                ) == EConfirmResult.Ok)
            {
                SaveMap();
            }
        }

        public void SaveMap(bool isSilent = false)
        {
            Campaign currentCampaign = GameManager.CurrentCampaign;

            string campaignDirectoryPath = FileOperationsHelper.CampaignDirectoryPath;

            campaignDirectoryPath.CreateDirectoryIfNotExists();

            currentCampaign.ReplaceMap(GameManager.CurrentMap);
            
            FileOperationsHelper.SaveCampaign(currentCampaign,
                () => EditorUIManager.MessageBar.Set(t.Get(Keys.SaveFailed), EMessageType.Negative, automaticDismissDelay: 3f));

            if (!isSilent)
            {
                EditorUIManager.MessageBar.Set(t.Get(Keys.MapSaved), EMessageType.Positive, automaticDismissDelay: 1f);
                EditorEvents.TriggerOnPrefabEdited(false);
            }
            
            MapIsChanged = false;
            _mapIsSaved = true;
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

        private void OnLayoutBuilt()
        {
            Vector3 startPosition = GameManager.CurrentMap.StartGridPosition;

            if (!MapIsPresented && !_dontChangeCameraAfterLayoutIsBuild)
            {
                EditorCameraService.Instance.MoveCameraTo(startPosition.y, cameraHeight, startPosition.z);
            }

            MapIsBeingBuilt = false;
            MapIsPresented = true;

            SetWorkMode(EWorkMode.Build);

            MapDescription map = GameManager.CurrentMap;

            playerIcon.SetPositionByGrid(map.StartGridPosition);
            playerIcon.SetArrowRotation(map.PlayerRotation);
            playerIcon.SetActive(true);

            EditorCameraService.Instance.ResetCamera();
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