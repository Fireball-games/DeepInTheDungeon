using Scripts.System.MonoBases;
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

        public void SetMainCamera(Camera newCamera = null, Enums.ECameraBackgroundMode backgroundMode = Enums.ECameraBackgroundMode.SolidColor)
        {
            if (mainCamera && newCamera == mainCamera) return;
            
            if (mainCamera)
            {
                mainCamera.enabled = false;
                mainCamera.tag = Helpers.Strings.Untagged;
            }
            newCamera = !newCamera ? _ownCamera : newCamera;
            mainCamera = newCamera;
            newCamera!.tag = Helpers.Strings.MainCamera;
            SetBackgroundMode(newCamera, backgroundMode);
            newCamera.enabled = true;
        }

        private static void SetBackgroundMode(Camera newCamera, Enums.ECameraBackgroundMode backgroundMode)
        {
            switch (backgroundMode)
            {
                case Enums.ECameraBackgroundMode.SolidColor:
                    newCamera.clearFlags = CameraClearFlags.SolidColor;
                    // newCamera.backgroundColor = Color.black;
                    break;
                case Enums.ECameraBackgroundMode.Skybox:
                    newCamera.clearFlags = CameraClearFlags.Skybox;
                    break;
                case Enums.ECameraBackgroundMode.Uninitialized:
                default:
                    newCamera.clearFlags = CameraClearFlags.SolidColor;
                    newCamera.backgroundColor = Color.magenta;
                    break;
            }
        }
    }
}
