using System;
using UnityEngine;

namespace Scripts
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera uiCamera;

        public static Camera MainCamera;

        private static event Action<ECameraType> OnMainCameraSet;

        private enum ECameraType
        {
            UI = 1,
            Main = 2,
        }

        private void OnEnable()
        {
            OnMainCameraSet += SwapCamera;
        }

        private void SwapCamera(ECameraType cameraType)
        {
            bool swappedToMain = cameraType is ECameraType.Main;

            uiCamera.enabled = !swappedToMain;
            MainCamera.enabled = swappedToMain;
        }

        public static void SetMainCamera(Camera newCamera)
        {
            MainCamera = newCamera;
            OnMainCameraSet?.Invoke(ECameraType.Main);
        }
    }
}
