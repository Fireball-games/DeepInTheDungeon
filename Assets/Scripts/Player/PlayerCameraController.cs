using Scripts.System.MonoBases;
using Scripts.UI.PlayMode;
using UnityEngine;

public class PlayerCameraController : SingletonNotPersisting<PlayerCameraController>
{
    private static PlayMouseService MouseService => PlayMouseService.Instance;
    private Transform _cameraArm;
    private Transform _cameraHolder;

    protected override void Awake()
    {
        base.Awake();
        
        _cameraArm = transform.Find("CameraArm");
        _cameraHolder = _cameraArm.Find("MainCamera");
    }

    private void Update()
    {
        
    }

    public void HandleMouseMovement()
    {
        
    }
}
