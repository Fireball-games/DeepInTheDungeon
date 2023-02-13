using System;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using UnityEngine;

namespace Scripts.Building
{
    public class EntryPoint : ICloneable
    {
        public string name;
        public Vector3 playerGridPosition;
        public int playerRotationY;
        public bool isMovingForwardOnStart;

        public EntryPoint()
        {
        }

        public EntryPoint(EntryPointConfiguration entryPointConfiguration)
        {
            name = entryPointConfiguration.EntryPointName;
            playerGridPosition = entryPointConfiguration.TransformData.Position.ToGridPosition();
            playerRotationY = entryPointConfiguration.PlayerRotationY;
            isMovingForwardOnStart = entryPointConfiguration.IsMovingForwardOnStart;
        }

        public object Clone() => new EntryPoint
        {
            name = name,
            playerGridPosition = playerGridPosition,
            playerRotationY = playerRotationY,
            isMovingForwardOnStart = isMovingForwardOnStart,
        };
    }
}