using System;
using Scripts.Building;
using Scripts.EventsManagement;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.MapEditor
{
    public class MapEditorManager : Singleton<MapEditorManager>
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private float cameraHeight = 10f;

        public EWorkMode WorkMode => _workMode; 
        
        private MapBuilder _mapBuilder;
        private EWorkMode _workMode;
        
        protected override void Awake()
        {
            base.Awake();
            sceneCamera ??= Camera.main;
            CameraManager.SetMainCamera(sceneCamera);

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
            sceneCamera.transform.position = new(startPosition.x, cameraHeight, startPosition.z);
        }
    }
}
