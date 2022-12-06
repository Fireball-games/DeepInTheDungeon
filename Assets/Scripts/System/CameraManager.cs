using System;
using Scripts.EventsManagement;
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

        public void SetMainCamera(Camera newCamera = null)
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
            newCamera.enabled = true;
        }
    }
}
