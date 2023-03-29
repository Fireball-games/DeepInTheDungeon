using System;
using Scripts.System;
using UnityEngine;

namespace Scripts.Helpers
{
    public class LayersManager : MonoBehaviour
    {
        public const string WallMaskName = "Wall";
        public const string WallGizmoMaskName = "WallGizmo";
        public const string ItemMaskName = "Item";
        private const string UpperFloorMaskName = "UpperFloor"; 
        public static int UpperFloor { get; private set; }
        public static int Item { get; private set; }

        private const float RayHitDistance = 100f;
        private static Ray MouseRay => CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);

        private void Awake()
        {
            UpperFloor = LayerMask.NameToLayer(UpperFloorMaskName);
            Item = LayerMask.NameToLayer(ItemMaskName);
        }

        public static bool CheckRayHit(string layerName, out GameObject hitObject)
        {
            if (Physics.Raycast(MouseRay, out RaycastHit hit, RayHitDistance, LayerMask.GetMask(layerName)))
            {
                hitObject = hit.collider.gameObject;
                return true;
            }

            hitObject = null;
            return false;
        } 
    }
}