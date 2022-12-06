using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.ScenesManagement;
using Scripts.System;
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
        public bool MapIsEdited { get; private set; }
        public bool MapIsChanged { get; set; }
        public bool MapIsBeingBuilt { get; private set; }
        public LayoutType EditedLayout { get; private set; }
        public MapBuilder MapBuilder { get; private set; }

        private EWorkMode _workMode;


        protected override void Awake()
        {
            base.Awake();
            
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);

            MapBuilder = GameController.Instance.MapBuilder;
        }

        private void OnEnable()
        {
            MapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnDisable()
        {
            MapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        public void OrderMapConstruction(MapDescription map = null)
        {
            MapIsBeingBuilt = true;
            MapIsEdited = false;

            if (MapBuilder.LayoutParent) MapBuilder.DemolishMap();

            MapDescription newMap = map ??= new MapDescription();

            EditedLayout = MapBuildService.ConvertToLayoutType(map.Layout);

            GameController.Instance.SetCurrentMap(newMap);

            MapBuilder.BuildMap(newMap);

            EditorEvents.TriggerOnNewMapCreated();
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            _workMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(_workMode);
        }
        
        public void PlayMap()
        {
            string mapName = GameController.Instance.CurrentMap.MapName;
            ES3.Save(mapName, GameController.Instance.CurrentMap, "Maps/mapName.map");
            
            SceneLoader.Instance.LoadMainScene(true);
        }

        private void OnLayoutBuilt()
        {
            MapIsBeingBuilt = false;
            MapIsEdited = true;
            SetWorkMode(EWorkMode.Build);
            Vector3 startPosition = GameController.Instance.CurrentMap.StartPosition;
            sceneCamera.transform.position = new Vector3(startPosition.x, cameraHeight, startPosition.z);
            playerIcon.transform.position = GameController.Instance.CurrentMap.StartPosition;
            // TODO: rotate by data from CurrentMap when implemented
            playerIcon.SetArrowRotation(Vector3.zero);
            playerIcon.SetActive(true);
        }
    }
}