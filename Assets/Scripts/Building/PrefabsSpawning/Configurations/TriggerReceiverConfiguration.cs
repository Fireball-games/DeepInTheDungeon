namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TriggerReceiverConfiguration : PrefabConfiguration
    {
        public int StartMovement;
        
        public TriggerReceiverConfiguration()
        {
        }

        public TriggerReceiverConfiguration(TriggerReceiverConfiguration receiverConfiguration) : base(receiverConfiguration)
        {
            StartMovement = receiverConfiguration.StartMovement;
        }
    }
}