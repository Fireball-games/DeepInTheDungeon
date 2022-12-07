using System;
using Scripts.System;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class EditorCameraController : MonoBehaviour
    {
        private Camera Camera => CameraManager.Instance.mainCamera;

        private void LateUpdate()
        {
            throw new NotImplementedException();
        }
    }
}