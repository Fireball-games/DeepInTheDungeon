using UnityEngine;

namespace Scripts.MapEditor
{
    public class EditorInputService : MonoBehaviour
    {
        [SerializeField] private Transform cameraHolder;
        private EditorMouseService Mouse => EditorMouseService.Instance;
        private void Update()
        {
            // Rotate camera 90° counter-clockwise
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 newRotation = cameraHolder.localRotation.eulerAngles;
                newRotation.y = (newRotation.y - newRotation.y % 90) - 90;
                cameraHolder.localRotation = Quaternion.Euler(newRotation);
            }
            // Rotate camera 90° clockwise
            if (Input.GetKeyDown(KeyCode.E))
            {
                Vector3 newRotation = cameraHolder.localRotation.eulerAngles;
                newRotation.y = (newRotation.y + 90) - (newRotation.y % 90);
                cameraHolder.localRotation = Quaternion.Euler(newRotation);
            }
            // Reset camera rotation
            if (Input.GetKeyDown(KeyCode.W))
            {
                cameraHolder.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }
}