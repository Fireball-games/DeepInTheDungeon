﻿using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.Triggers;
using static Scripts.Enums;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TriggerReceiverConfiguration : PrefabConfiguration
    {
        public int StartMovement;

        public TriggerReceiverConfiguration(TriggerReceiver receiver, string ownerGuid = null, bool spawnPrefabOnBuild = true)
        {
            StartMovement = receiver.startMovement;

            PrefabName = string.IsNullOrEmpty(receiver.identification) ? global::System.Guid.NewGuid().ToString() : receiver.identification;
            TransformData = new PositionRotation(receiver.transform.position.Round(2), receiver.transform.rotation);
            PrefabType = EPrefabType.TriggerReceiver;
            Guid = string.IsNullOrEmpty(receiver.Guid) ? global::System.Guid.NewGuid().ToString() : receiver.Guid;
            SpawnPrefabOnBuild = spawnPrefabOnBuild;
            OwnerGuid = ownerGuid;
        }
    }
}