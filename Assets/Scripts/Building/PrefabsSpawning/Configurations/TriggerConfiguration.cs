using System.Collections.Generic;
using System.Linq;
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

        public TriggerConfiguration(Trigger trigger, bool spawnPrefabOnBuild = true) : base(trigger, spawnPrefabOnBuild)
        {
            Subscribers = trigger.presetSubscribers.Select(s => s.GUID).ToList();
        }
    }
}