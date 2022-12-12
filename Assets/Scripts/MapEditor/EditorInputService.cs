using System;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class EditorInputService : MonoBehaviour
    {
        [SerializeField] private Transform cameraRotator;
        private EditorMouseService Mouse => EditorMouseService.Instance;
        private void Update()
        {
            // Rotate camera 90° counter-clockwise
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 newRotation = cameraRotator.localRotation.eulerAngles;
                newRotation.y = (newRotation.y - newRotation.y % 90) - 90;
                cameraRotator.localRotation = Quaternion.Euler(newRotation);
            }
            // Rotate camera 90° clockwise
            if (Input.GetKeyDown(KeyCode.E))
            {
                Vector3 newRotation = cameraRotator.localRotation.eulerAngles;
                newRotation.y = (newRotation.y + 90) - (newRotation.y % 90);
                cameraRotator.localRotation = Quaternion.Euler(newRotation);
            }
            // Reset camera rotation
            if (Input.GetKeyDown(KeyCode.W))
            {
                cameraRotator.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }
}