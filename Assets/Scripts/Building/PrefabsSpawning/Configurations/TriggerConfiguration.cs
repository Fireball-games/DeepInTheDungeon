using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Triggers;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TriggerConfiguration : PrefabConfiguration
    {
        public List<string> Subscribers;

        public TriggerConfiguration()
        {
        }

        public TriggerConfiguration(TriggerConfiguration receiverConfiguration) : base(receiverConfiguration)
        {
            Subscribers = receiverConfiguration.Subscribers;
        }

        public TriggerConfiguration(Trigger trigger, string ownerGuid = null, bool spawnPrefabOnBuild = true) : base(trigger, ownerGuid, spawnPrefabOnBuild)
        {
            Subscribers = trigger.presetSubscribers.Select(s => s.GUID).ToList();
            TransformData.Position = trigger.transform.position.Round(2);
        }
    }
}