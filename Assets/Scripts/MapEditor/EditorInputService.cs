using UnityEngine;

namespace Scripts.MapEditor
{
    public class EditorInputService : MonoBehaviour
    {
        [SerializeField] private float keyboardSpeed = 10f;
        [SerializeField] private Transform cameraHolder;
        private EditorMouseService Mouse => EditorMouseService.Instance;
        private void Update()
        {
            if (!Mouse.IsManipulatingCameraPosition)
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
            else
            {
                Vector3 move = Vector3.zero;
                Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
                Vector3 localForward = worldToLocalMatrix.MultiplyVector(-cameraHolder.forward);
                Vector3 localRight = worldToLocalMatrix.MultiplyVector(cameraHolder.right);
                
                if (Input.GetKey(KeyCode.A))
                {
                    move += localRight * (keyboardSpeed * Time.deltaTime);
                }
                // Move Camera right
                if (Input.GetKey(KeyCode.D))
                {
                    move += localRight * (-keyboardSpeed * Time.deltaTime);
                }
                // Move Camera forward
                if (Input.GetKey(KeyCode.W))
                {
                    move += localForward * (keyboardSpeed * Time.deltaTime);
                }
                // Move Camera backward
                if (Input.GetKey(KeyCode.S))
                {
                    move += localForward * (-keyboardSpeed * Time.deltaTime);
                }

                move.y = 0;
                
                // Move Camera down
                if (Input.GetKey(KeyCode.Q))
                {
                    move += Vector3.down * (keyboardSpeed * Time.deltaTime);
                }
                // Move Camera up
                if (Input.GetKey(KeyCode.E))
                {
                    move += Vector3.up * (keyboardSpeed * Time.deltaTime);
                }
                
                EditorCameraService.Instance.TranslateCamera(move);
            }
        }
    }
}