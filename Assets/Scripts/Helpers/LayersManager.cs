using Scripts.System;
using UnityEngine;

namespace Scripts.Helpers
{
    public class LayersManager : MonoBehaviour
    {
        public const string WallMaskName = "Wall";
        public const string WallGizmoMaskName = "WallGizmo";
        
        private static float rayHitDistance = 100f;
        private static Ray MouseRay => CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);

        public static bool CheckRayHit(string layerName, out GameObject hitObject)
        {
            if (Physics.Raycast(MouseRay, out RaycastHit hit, rayHitDistance, LayerMask.GetMask(layerName)))
            {
                hitObject = hit.collider.gameObject;
                return true;
            }

            hitObject = null;
            return false;
        } 
    }
}