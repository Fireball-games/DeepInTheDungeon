using DG.Tweening;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.PlayMode;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

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
    [SerializeField] private float minLeanXRotation = 0f;
    [SerializeField] private float maxLeanXRotation = 50f;
    
    private Quaternion originalRotation;
    private float currentZRotation;
    private float currentXRotation;

    public bool isLeaning;
    
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
                Logger.Log("resetting camera from look mode off");
                ResetCameraHolder();
            }
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
            HandleMouseMovement();
        }
        else
        {
            if (_cameraAtRest 
                && !isLeaning 
                && MouseService.RightClickExpired 
                && Input.GetMouseButton(1))
            {
                HandleMouseMovement();
            }

            if (Input.GetMouseButtonUp(1) && _cameraHolder.localRotation.eulerAngles != Vector3.zero)
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
        
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

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

        currentZRotation = _cameraArm.localEulerAngles.z;
        currentXRotation = _cameraArm.localEulerAngles.x;
        
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
}
