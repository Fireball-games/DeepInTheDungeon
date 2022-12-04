using System;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.MapEditor
{
    public class MapEditorManager : Singleton<MapEditorManager>
    {
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private PlayerIconController playerIcon;

        public EWorkMode WorkMode => _workMode;
        public bool MapIsEdited { get; private set; }

        private MapBuilder _mapBuilder;
        private EWorkMode _workMode;
        
        protected override void Awake()
        {
            base.Awake();
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);

            _mapBuilder ??= GameController.Instance.MapBuilder;
        }

        private void OnEnable()
        {
            _mapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnDisable()
        {
            _mapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        public void CreateNewMap()
        {
            MapIsEdited = true;
            
            EditorEvents.TriggerOnNewMapCreated();

            MapDescription newMap = new();
            
            GameController.Instance.SetCurrentMap(newMap);
            
            _mapBuilder.BuildMap(newMap);
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            _workMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(_workMode);
        }

        private void OnLayoutBuilt()
        {
            Vector3 startPosition = GameController.Instance.CurrentMap.StartPosition;
            sceneCamera.transform.position = new Vector3(startPosition.x, cameraHeight, startPosition.z);
            playerIcon.transform.position = GameController.Instance.CurrentMap.StartPosition;
            // TODO: rotate by data from CurrentMap when implemented
            playerIcon.SetArrowRotation(Vector3.zero);
            playerIcon.SetActive(true);
        }
    }
}
