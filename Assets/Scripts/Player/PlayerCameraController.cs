using DG.Tweening;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.PlayMode;
using UnityEngine;

public class PlayerCameraController : SingletonNotPersisting<PlayerCameraController>
{
    [SerializeField] private float lookSpeed = 150f;
    [SerializeField] private float minXRotation = -60f;
    [SerializeField] private float maxXRotation = 60f;
    [SerializeField] private float minYRotation = -60f;
    [SerializeField] private float maxYRotation = 65f;

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
        }
    }
    
    private static PlayMouseService MouseService => PlayMouseService.Instance;
    private Transform _cameraArm;
    private Transform _cameraHolder;

    private bool _cameraAtRest = true;

    protected override void Awake()
    {
        base.Awake();

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
            if (_cameraAtRest && MouseService.RightClickExpired && Input.GetMouseButton(1))
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
        ResetCameraHolder();
    }

    private void ResetCameraHolder()
    {
        _cameraAtRest = false;
        _cameraHolder.DOLocalRotate(Vector3.zero, 0.5f)
            .OnComplete(() => { _cameraAtRest = true; MouseCursorManager.ResetCursor(); })
            .Play();
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
}
