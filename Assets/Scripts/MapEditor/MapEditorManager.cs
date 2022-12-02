using UnityEngine;

namespace Scripts.MapEditor
{
    public class MapEditorManager : Singleton<MapEditorManager>
    {
        [SerializeField] private Camera sceneCamera;
        
        protected override void Awake()
        {
            base.Awake();
            sceneCamera ??= Camera.main;
            CameraManager.SetMainCamera(sceneCamera);
        }
    }
}
