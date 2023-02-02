﻿// (c) Copyright Cleverous 2023. All rights reserved.

using UnityEngine;

namespace Cleverous.VaultInventory.Example
{
    public class VaultExampleCameraFollow : MonoBehaviour
    {
        public Camera TargetCamera;
        public float CameraOffset;
        public float Speed = 5;

        private GameObject m_focalPoint;
        private Vector3 m_offsetDir;

        public void Reset()
        {
            CameraOffset = 6;
        }

        public void Awake()
        {
            m_offsetDir = Vector3.back / 1.6f + Vector3.up;
            VaultInventory.OnPlayerSpawn += AssignPlayer;
        }

        public void AssignPlayer(IUseInventory player)
        {
            m_focalPoint = player.MyTransform.gameObject;
            TargetCamera.transform.position = m_focalPoint.transform.position + m_offsetDir * CameraOffset;
            transform.LookAt(m_focalPoint.transform);
        }

        public void LateUpdate()
        {
            if (m_focalPoint == null) return;
            TargetCamera.transform.position = Vector3.Lerp(
                transform.position, 
                m_focalPoint.transform.position + m_offsetDir * CameraOffset, 
                Time.deltaTime * Speed);
        }
    }
}