using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.System.MouseCursorManager;

namespace Scripts.UI.PlayMode
{
    public class PlayMouseService : MouseServiceBase<PlayMouseService>
    {
        private static GameManager Manager => GameManager.Instance;
        private static PlayerCameraController PlayerCamera => PlayerCameraController.Instance;
        
        private void Update()
        {
            if (!Manager) return;

            ValidateClicks();
            PlayerCamera.HandleMouseMovement();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                UIIsBlocking = true;
                SetDefaultCursor();

                return;
            }
            
            if (UIIsBlocking)
            {
                UIIsBlocking = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                
            }

            if (!LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                ProcessMouseButtonUp(0);
            }
            else
            {
                PlayerCamera.HandleMouseMovement();
            }
        }

        private void ProcessMouseButtonUp(int buttonNumber)
        {
            
        }
    }
}