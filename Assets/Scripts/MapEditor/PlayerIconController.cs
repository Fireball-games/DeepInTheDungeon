using Scripts.Helpers;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class PlayerIconController : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject arrow;

        public void SetPositionByGrid(Vector3Int gridPosition)
        {
            gridPosition = gridPosition.SwapXY();
            gridPosition.y = -gridPosition.y;
            gameObject.transform.position = gridPosition;
        }

        public void SetArrowRotation(Vector3 rotation)
        {
            Vector3 arrowRotation = new(0f, 0f, rotation.y);
            arrow.transform.localRotation = Quaternion.Euler(arrowRotation);
        }

        public void SetActive(bool isActive) => body.SetActive(isActive);
    }
}
