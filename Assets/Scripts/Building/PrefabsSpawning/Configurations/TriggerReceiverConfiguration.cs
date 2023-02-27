using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.Triggers;
using static Scripts.Enums;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TriggerReceiverConfiguration : PrefabConfiguration
    {
        public int CurrentPosition;
        public string Identification;

        public TriggerReceiverConfiguration(TriggerReceiver receiver, string ownerGuid = null, bool spawnPrefabOnBuild = true)
        {
            CurrentPosition = receiver.CurrentPosition;
            Identification = receiver.identification;

            PrefabName = string.IsNullOrEmpty(receiver.identification) ? global::System.Guid.NewGuid().ToString() : receiver.identification;
            TransformData = new PositionRotation(receiver.transform.position.Round(2), receiver.transform.rotation);
            PrefabType = EPrefabType.TriggerReceiver;
            Guid = string.IsNullOrEmpty(receiver.Guid) ? global::System.Guid.NewGuid().ToString() : receiver.Guid;
            SpawnPrefabOnBuild = spawnPrefabOnBuild;
            OwnerGuid = ownerGuid;
        }
    }
}