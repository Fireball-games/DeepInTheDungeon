using UnityEngine;

namespace Scripts.System
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera MainCamera;
        private Camera ownCamera;

        protected override void Awake()
        {
            base.Awake();

            ownCamera = GetComponent<Camera>();
            
            SetMainCamera(ownCamera);
        }

        public void SetMainCamera(Camera newCamera)
        {
            if (MainCamera)
            {
                MainCamera.enabled = false;
                MainCamera.tag = Helpers.Strings.Untagged;
            }
            
            MainCamera = newCamera;
            newCamera.tag = Helpers.Strings.MainCamera;
            newCamera.enabled = true;

        }
    }
}
