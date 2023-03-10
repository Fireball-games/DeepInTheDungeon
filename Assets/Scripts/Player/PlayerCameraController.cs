using DG.Tweening;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.PlayMode;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerCameraController : SingletonNotPersisting<PlayerCameraController>
    {
        [Header("Free Look Mode settings")]
        [SerializeField] private float lookSpeed = 150f;
        [SerializeField] private float minXRotation = -60f;
        [SerializeField] private float maxXRotation = 60f;
        [SerializeField] private float minYRotation = -60f;
        [SerializeField] private float maxYRotation = 65f;

        [Header("Leaning settings")] [SerializeField]
        private float leanSpeed = 100f;
        [SerializeField] private float leanAngle = 10f;
        [SerializeField] private float upLeanAngle = 50f;
        [SerializeField] private float minLeanZRotation = -10f;
        [SerializeField] private float maxLeanZRotation = 10f;
        [SerializeField] private float minLeanXRotation;
        [SerializeField] private float maxLeanXRotation = 50f;
    
        private Quaternion originalRotation;
        private float currentZRotation;
        private float currentXRotation;

        public bool isLeaning;

        private bool _isLookModeFromRightClick;
        private bool _isLookModeOn;
        public bool IsLookModeOn
        {
            get => _isLookModeOn;
            set
            {
                if (_isLookModeOn == value) return;

                _isLookModeOn = value;
            
                if (_isLookModeOn)
                {
                    MouseCursorManager.HideAndLock();
                }
                else
                {
                    ResetCameraHolder();
                }
            
                PlayEvents.TriggerOnLookModeActiveChanged(_isLookModeOn);
            }
        }
    
        private static PlayMouseService MouseService => PlayMouseService.Instance;
        private Transform _cameraArm;
        private Transform _cameraHolder;

        private bool _cameraAtRest = true;

        protected override void Awake()
        {
            base.Awake();

            originalRotation = Quaternion.Euler(Vector3.zero);
            _cameraArm = transform.Find("CameraArm");
            _cameraHolder = _cameraArm.Find("MainCamera");
        }

        private void Update()
        {
            if (!MouseService) return;

            if (IsLookModeOn)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    if (_isLookModeFromRightClick)
                    {
                        _isLookModeFromRightClick = false;
                        IsLookModeOn = false;
                        return;
                    }
                }
            
                HandleMouseMovement();
            }
            else
            {
                if (_cameraAtRest 
                    && !IsLookModeOn
                    && !isLeaning 
                    && MouseService.RightClickExpired 
                    && Input.GetMouseButton(1))
                {
                    IsLookModeOn = true;
                    _isLookModeFromRightClick = true;
                    HandleMouseMovement();
                }

                if (Input.GetMouseButtonUp(1))
                {
                    ResetCameraHolder();
                }
            }
        }

        public void ResetCamera()
        {
            if (!_cameraAtRest) return;
        
            ResetCameraHolder();
        }
        
        public void SetRotationLimits(RotationSettings rotationSettings)
        {
            minXRotation = rotationSettings.MinXRotation;
            maxXRotation = rotationSettings.MaxXRotation;
            minYRotation = rotationSettings.MinYRotation;
            maxYRotation = rotationSettings.MaxYRotation;
        }

        private void ResetCameraHolder()
        {
            if (_cameraHolder.localRotation != Quaternion.Euler(Vector3.zero))
            {
                _cameraAtRest = false;
                _cameraHolder.DOLocalRotate(Vector3.zero, 0.5f)
                    .OnComplete(() =>
                    {
                        _cameraAtRest = true;
                        MouseCursorManager.ResetCursor();
                    })
                    .Play();
            }
        
            if (_cameraArm.localRotation != Quaternion.Euler(Vector3.zero))
            {
                _cameraAtRest = false;
                _cameraArm.DOLocalRotate(Vector3.zero, 0.5f)
                    .OnComplete(() =>
                    {
                        _cameraAtRest = true;
                        MouseCursorManager.ResetCursor();
                    })
                    .Play();
            }
        }

        private void HandleMouseMovement()
        {
            MouseCursorManager.HideAndLock();
        
            float mouseX = Input.GetAxis(Strings.MouseXAxis) * lookSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis(Strings.MouseYAxis) * lookSpeed * Time.deltaTime;

            Quaternion xQuaternion = Quaternion.AngleAxis(mouseX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(mouseY, Vector3.left);

            Quaternion newRotation = _cameraHolder.localRotation * xQuaternion * yQuaternion;

            float xRotation = newRotation.eulerAngles.x;
            float yRotation = newRotation.eulerAngles.y;

            if (xRotation > 180)
            {
                xRotation -= 360;
            }

            if (yRotation > 180)
            {
                yRotation -= 360;
            }

            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
            yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);

            newRotation = Quaternion.Euler(xRotation, yRotation, 0);

            _cameraHolder.localRotation = newRotation;
        }

        public void Lean(bool isLeaningForward, bool isLeaningLeft, bool isLeaningRight)
        {
            if (isLeaningLeft)
            {
                Quaternion leanLeft = Quaternion.AngleAxis(Mathf.Lerp(currentZRotation, leanAngle, leanSpeed * Time.deltaTime), Vector3.forward);
                _cameraArm.localRotation = originalRotation * leanLeft;
            } else if (isLeaningRight)
            {
                Quaternion leanRight = Quaternion.AngleAxis(Mathf.Lerp(currentZRotation, -leanAngle, leanSpeed * Time.deltaTime), Vector3.forward);
                _cameraArm.localRotation = originalRotation * leanRight;
            } else if (isLeaningForward)
            {
                Quaternion leanUp = Quaternion.AngleAxis(Mathf.Lerp(currentXRotation,upLeanAngle, leanSpeed * Time.deltaTime), Vector3.right);
                _cameraArm.localRotation = originalRotation * leanUp;
            }
            else
            {
                ResetCamera();
            }

            Vector3 localEulerAngles = _cameraArm.localEulerAngles;
            currentZRotation = localEulerAngles.z;
            currentXRotation = localEulerAngles.x;
        
            if (currentZRotation > 180)
            {
                currentZRotation -= 360;
            }
            if (currentXRotation > 180)
            {
                currentXRotation -= 360;
            }
            currentZRotation = Mathf.Clamp(currentZRotation, minLeanZRotation, maxLeanZRotation);
            currentXRotation = Mathf.Clamp(currentXRotation, minLeanXRotation, maxLeanXRotation);

            _cameraArm.localRotation = Quaternion.Euler(currentXRotation, 0, currentZRotation);
        }

        public void HandleLookModeOnKeyClick()
        {
            if (_isLookModeFromRightClick)
            {
                _isLookModeFromRightClick = false;
            }
            else
            {
                IsLookModeOn = !IsLookModeOn;
            }
        }
    }

    public class RotationSettings
    {
        public float MinXRotation;
        public float MaxXRotation;
        public float MinYRotation;
        public float MaxYRotation;
    }
}
