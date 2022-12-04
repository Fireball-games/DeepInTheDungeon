using UnityEngine;

namespace Scripts.MapEditor
{
    public class PlayerIconController : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject arrow;

        public void SetArrowRotation(Vector3 rotation)
        {
            Vector3 arrowRotation = new(0f, 0f, rotation.y);
        }

        public void SetActive(bool isActive) => body.SetActive(isActive);
    }
}
