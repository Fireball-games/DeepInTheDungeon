using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.MapEditor
{
    public class EditorStartIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject arrow;

        public void SetPositionByGrid(Vector3Int gridPosition)
        {
            // gridPosition = gridPosition.SwapXY();
            // gridPosition.y = -gridPosition.y;
            SetPositionByWorld(gridPosition.ToWorldPosition());
        }
        
        public void SetPositionByWorld(Vector3 worldPosition) => gameObject.transform.position = worldPosition;
        
        public void SetArrowRotation(Quaternion rotation) => SetArrowRotation(rotation.eulerAngles);
        /// <summary>
        /// Sets rotation of the arrow by the given rotation.
        /// </summary>
        /// <param name="y"></param>
        public void SetArrowRotationYDelta(float y)
        {
            SetArrowRotation(new Vector3(0f, arrow.transform.localRotation.eulerAngles.y + y, 0f));
        }

        private void SetArrowRotation(Vector3 rotation)
        {
            Vector3 arrowRotation = rotation;
            arrowRotation.x = 90f;
            arrowRotation.z = rotation.y - 90;
            arrow.transform.localRotation = Quaternion.Euler(arrowRotation);
            GameManager.Instance.CurrentMap.EditorPlayerStartRotation = Quaternion.Euler(rotation);
        }

        public void SetActive(bool isActive) => body.SetActive(isActive);
    }
}
