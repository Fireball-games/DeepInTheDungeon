﻿using System;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.System;
using Unity.Mathematics;
using UnityEngine;

namespace Scripts.Building
{
    public class EntryPoint : ICloneable
    {
        public string name;
        public Vector3 playerLocation;
        public Quaternion playerRotation;
        public bool isMovingForwardOnStart;

        public object Clone() => new EntryPoint
        {
            name = name,
            playerLocation = playerLocation,
            playerRotation = playerRotation,
            isMovingForwardOnStart = isMovingForwardOnStart,
        };
        
        public EntryPointConfiguration ToConfiguration() => new()
        {
            EntryPointName = name,
            TransformData = new PositionRotation(playerLocation, quaternion.identity),
            LookDirection = playerRotation,
            IsMovingForwardOnStart = isMovingForwardOnStart,
        };
    }
}