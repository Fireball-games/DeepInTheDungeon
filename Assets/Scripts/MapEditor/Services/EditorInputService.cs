using UnityEngine;

namespace Scripts.MapEditor.Services
{
    public class EditorInputService : MonoBehaviour
    {
        [SerializeField] private float keyboardSpeed = 10f;
        [SerializeField] private Transform cameraHolder;
        private EditorMouseService Mouse => EditorMouseService.Instance;
        private EditorCameraService CameraService => EditorCameraService.Instance;
        private void Update()
        {
            if (!Mouse || !Input.GetKey(KeyCode.LeftControl)) return;
            
            if (!Mouse.IsManipulatingCameraPosition)
            {
                // Rotate camera 90° counter-clockwise
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Vector3 newRotation = cameraHolder.localRotation.eulerAngles;
                    newRotation.y = (newRotation.y - newRotation.y % 90) - 90;
                    CameraService.RotateCameraSmooth(newRotation);
                }
                // Rotate camera 90° clockwise
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Vector3 newRotation = cameraHolder.localRotation.eulerAngles;
                    newRotation.y = (newRotation.y + 90) - (newRotation.y % 90);
                    CameraService.RotateCameraSmooth(newRotation);
                }
                // Reset camera rotation
                if (Input.GetKeyDown(KeyCode.W))
                {
                    EditorCameraService.Instance.ResetCamera();
                }
                // Toggles Perspective/Orthographic camera
                if (Input.GetKeyDown(KeyCode.C))
                {
                    EditorCameraService.ToggleCameraPerspective();
                }
            }
            else
            {
                bool doMove = false;
                Vector3 move = Vector3.zero;
                Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
                Vector3 localForward = worldToLocalMatrix.MultiplyVector(-cameraHolder.forward);
                Vector3 localRight = worldToLocalMatrix.MultiplyVector(cameraHolder.right);
                
                if (Input.GetKey(KeyCode.A))
                {
                    move += localRight * (keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }
                // Move Camera right
                if (Input.GetKey(KeyCode.D))
                {
                    move += localRight * (-keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }
                // Move Camera forward
                if (Input.GetKey(KeyCode.W))
                {
                    move += localForward * (keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }
                // Move Camera backward
                if (Input.GetKey(KeyCode.S))
                {
                    move += localForward * (-keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }

                move.y = 0;
                
                // Move Camera down
                if (Input.GetKey(KeyCode.Q))
                {
                    move += Vector3.down * (keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }
                // Move Camera up
                if (Input.GetKey(KeyCode.E))
                {
                    move += Vector3.up * (keyboardSpeed * Time.deltaTime);
                    doMove = true;
                }

                if (doMove)
                {
                    EditorCameraService.Instance.TranslateCamera(move, false);
                }
            }
        }
    }
}