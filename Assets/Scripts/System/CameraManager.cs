using UnityEngine;

namespace Scripts.System
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera mainCamera;
        private Camera _ownCamera;

        protected override void Awake()
        {
            base.Awake();

            _ownCamera = mainCamera;
            
            SetMainCamera(mainCamera);
        }

        public void SetMainCamera(Camera newCamera)
        {
            if (newCamera == mainCamera) return;
            
            if (mainCamera)
            {
                mainCamera.enabled = false;
                mainCamera.tag = Helpers.Strings.Untagged;
            }
            
            mainCamera = newCamera;
            newCamera.tag = Helpers.Strings.MainCamera;
            newCamera.enabled = true;
        }
    }
}
