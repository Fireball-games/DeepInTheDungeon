using Scripts.Building;
using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class MapEditorManager : Singleton<MapEditorManager>
    {
        [SerializeField] private Camera sceneCamera;

        private MapBuilder _mapBuilder;
        
        protected override void Awake()
        {
            base.Awake();
            sceneCamera ??= Camera.main;
            CameraManager.SetMainCamera(sceneCamera);

            _mapBuilder ??= FindObjectOfType<MapBuilder>(true);
        }

        public void CreateNewMap()
        {
            EditorEvents.TriggerOnNewMapCreated();

            MapDescription newMap = new();
            
            GameController.Instance.SetCurrentMap(newMap);
            
            _mapBuilder.BuildMap(newMap);
        }
    }
}
