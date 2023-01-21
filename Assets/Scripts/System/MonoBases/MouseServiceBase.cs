using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.System.MonoBases
{
    public abstract class MouseServiceBase<T> : SingletonNotPersisting<T> where T : MonoBehaviour
    {
        [SerializeField] private float maxValidClickTime = 0.1f;
        
        public bool LeftClickExpired { get; private set; }
        public bool RightClickExpired { get; private set; }
        public bool LeftClickedOnUI { get; private set; }
        
        protected bool UIIsBlocking;
        
        private float _lastLeftClickTime;
        private float _lastRightClickTime;
        private bool _isManipulatingCameraPosition;
        public bool IsManipulatingCameraPosition
        {
            get => _isManipulatingCameraPosition;
            set
            {
                if (value == _isManipulatingCameraPosition) return;

                if (!_isManipulatingCameraPosition)
                {
                    ResetCursor();
                }

                _isManipulatingCameraPosition = value;
            }
        }
        
        public virtual void ResetCursor()
        {
            MouseCursorManager.ResetCursor();
        }
        
        protected void ValidateClicks()
        {
            float currentTime = Time.time;

            if (Input.GetMouseButtonDown(0))
            {
                LeftClickExpired = false;
                _lastLeftClickTime = Time.time;

                LeftClickedOnUI = EventSystem.current.IsPointerOverGameObject();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RightClickExpired = false;
                _lastRightClickTime = Time.time;
            }

            if (!LeftClickExpired && currentTime - _lastLeftClickTime > maxValidClickTime)
            {
                LeftClickExpired = true;
            }

            if (!RightClickExpired && currentTime - _lastRightClickTime > maxValidClickTime)
            {
                RightClickExpired = true;
            }
        }
    }
}