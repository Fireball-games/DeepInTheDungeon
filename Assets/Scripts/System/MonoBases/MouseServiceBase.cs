﻿using UnityEngine;
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
        
        protected float LastLeftClickTime;
        protected float LastRightClickTime;
        
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
                LastLeftClickTime = Time.time;

                LeftClickedOnUI = EventSystem.current.IsPointerOverGameObject();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RightClickExpired = false;
                LastRightClickTime = Time.time;
            }

            if (!LeftClickExpired && currentTime - LastLeftClickTime > maxValidClickTime)
            {
                LeftClickExpired = true;
            }

            if (!RightClickExpired && currentTime - LastRightClickTime > maxValidClickTime)
            {
                RightClickExpired = true;
            }
        }
    }
}