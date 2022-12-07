using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Localization;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.UI.Components;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class MapEditorManager : SingletonNotPersisting<MapEditorManager>
    {
        public const int MinRows = 5;
        public const int MinColumns = 5;
        
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private PlayerIconController playerIcon;

        public EWorkMode WorkMode => _workMode;
        public bool MapIsPresented { get; private set; }
        public bool MapIsChanged { get; set; }
        public bool MapIsSaved { get; set; } = true;
        public static bool MapIsBeingBuilt { get; private set; }
        public LayoutType EditedLayout { get; private set; }
        public MapBuilder MapBuilder { get; private set; }

        private EWorkMode _workMode;
        
        protected override void Awake()
        {
            base.Awake();
            
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);

            MapBuilder = GameManager.Instance.MapBuilder;
        }

        private void OnEnable()
        {
            MapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnDisable()
        {
            MapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        public void OrderMapConstruction(MapDescription map, bool markMapAsSaved = false)
        {
            if (MapIsBeingBuilt) return;
            
            MapIsBeingBuilt = true;
            MapIsPresented = false;
            MapIsSaved = markMapAsSaved;
            
            EditedLayout = MapBuildService.ConvertToLayoutType(map.Layout);

            GameManager.Instance.SetCurrentMap(map);

            MapBuilder.BuildMap(map);

            EditorEvents.TriggerOnNewMapCreated();
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            _workMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(_workMode);
        }

        public void GoToMainMenu()
        {
            if (!MapIsSaved)
            {
                EditorUIManager.Instance.ConfirmationDialog.Open(
                    T.Get(LocalizationKeys.SaveEditedMapPrompt),
                    GoToMainScreenWithSave,
                    LoadMainSceneClear);
                
                return;
            }
            
            LoadMainSceneClear();
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
        
        public void PlayMap()
        {
            if (!MapIsPresented)
            {
                EditorUIManager.Instance.StatusBar.RegisterMessage(T.Get(LocalizationKeys.NoMapToPlayLoaded), StatusBar.EMessageType.Negative);
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
            ES3.Save(mapName, currentMap, FileOperationsHelper.GetSavePath(mapName));

            MapIsChanged = false;
            MapIsSaved = true;
        }

        private void OnLayoutBuilt()
        {
            MapIsBeingBuilt = false;
            MapIsPresented = true;

            SetWorkMode(EWorkMode.Build);
            Vector3 startPosition = GameManager.Instance.CurrentMap.StartPosition;
            sceneCamera.transform.position = new Vector3(startPosition.x, cameraHeight, startPosition.z);
            playerIcon.transform.position = GameManager.Instance.CurrentMap.StartPosition;
            // TODO: rotate by data from CurrentMap when implemented
            playerIcon.SetArrowRotation(Vector3.zero);
            playerIcon.SetActive(true);
        }
    }
}