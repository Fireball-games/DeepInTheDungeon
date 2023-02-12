using System;
using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine;

namespace Scripts.Building
{
    public class EntryPoint : ICloneable
    {
        public string name;
        public Vector3 playerLocation;
        public Quaternion playerRotation;
        public bool isMovingForwardOnStart;

        public EntryPoint()
        {
        }

        public EntryPoint(EntryPointConfiguration entryPointConfiguration)
        {
            name = entryPointConfiguration.EntryPointName;
            playerLocation = entryPointConfiguration.TransformData.Position;
            playerRotation = entryPointConfiguration.LookDirection;
            isMovingForwardOnStart = entryPointConfiguration.IsMovingForwardOnStart;
        }

        public object Clone() => new EntryPoint
        {
            name = name,
            playerLocation = playerLocation,
            playerRotation = playerRotation,
            isMovingForwardOnStart = isMovingForwardOnStart,
        };
    }
}