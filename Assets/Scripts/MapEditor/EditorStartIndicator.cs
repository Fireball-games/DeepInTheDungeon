using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class EditorStartIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject arrow;

        public void SetPositionByGrid(Vector3Int gridPosition)
        {
            SetPositionByWorld(gridPosition.ToWorldPosition());
        }
        
        public void SetPositionByWorld(Vector3 worldPosition)
        {
            gameObject.transform.position = worldPosition;
        }
        
        public void SetPositionInMapAndWorld(Vector3 worldPosition)
        {
            GameManager.Instance.CurrentMap.EditorStartPosition = worldPosition.ToGridPosition();
            SetPositionByWorld(worldPosition);
        }

        public void SetArrowRotation(Quaternion rotation) => SetArrowRotation(rotation.eulerAngles);
        
        /// <summary>
        /// Sets rotation of the arrow by the given rotation.
        /// </summary>
        /// <param name="y"></param>
        public void SetArrowRotationYDelta(float y) => arrow.transform.localRotation *= Quaternion.AngleAxis(y, Vector3.up);

        private void SetArrowRotation(Vector3 rotation) => arrow.transform.localRotation = Quaternion.Euler(new Vector3(0f, rotation.y + 90, 0f));

        public void SetActive(bool isActive) => body.SetActive(isActive);

        /// <summary>
        /// Gets translated rotation for player at start, inferred from arrow rotation.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetPlayerMapRotation() => Quaternion.Euler(new Vector3(0f, arrow.transform.localRotation.eulerAngles.y - 90, 0f));
    }
}
