using System.Collections.Generic;

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
    }
}