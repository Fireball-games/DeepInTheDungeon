using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    private Transform _cameraArm;
    private Transform _cameraHolder;

    private void Awake()
    {
        _cameraArm = transform.Find("CameraArm");
        _cameraHolder = _cameraArm.Find("MainCamera");
    }

    private void Update()
    {
        throw new NotImplementedException();
    }
}
