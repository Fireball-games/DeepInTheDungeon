using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.EventSystems;
using static Scripts.System.MouseCursorManager;

namespace Scripts.PlayMode
{
    public class PlayMouseService : MouseServiceBase<PlayMouseService>
    {
        private static GameManager Manager => GameManager.Instance;
        
        private void Update()
        {
            if (!Manager) return;

            ValidateClicks();

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

            if (!LeftClickExpired && Input.GetMouseButtonUp(0))
            {
                ProcessMouseButtonUp(0);
            }
        }

        private void ProcessMouseButtonUp(int buttonNumber)
        {
            
        }
    }
}