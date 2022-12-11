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
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 newRotation = cameraRotator.localRotation.eulerAngles;
                newRotation.y -= 90;
                cameraRotator.localRotation = Quaternion.Euler(newRotation);
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Vector3 newRotation = cameraRotator.localRotation.eulerAngles;
                newRotation.y += 90;
                cameraRotator.localRotation = Quaternion.Euler(newRotation);
            }
        }
    }
}