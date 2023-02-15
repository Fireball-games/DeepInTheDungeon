using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Triggers;

namespace Scripts.Building.PrefabsSpawning.Configurations
{
    public class TriggerConfiguration : PrefabConfiguration
    {
        public List<string> Subscribers;
        public Enums.ETriggerType TriggerType;
        /// <summary>
        /// How many times can be trigger triggered.
        /// </summary>
        public int Count;
        public int StartPosition;
        
        //MapTraversalTriggerProperties
        public string TargetMap;
        public string TargetMapEntrance;
        public float Delay;

        public TriggerConfiguration()
        {
        }

        public TriggerConfiguration(TriggerConfiguration configuration) : base(configuration)
        {
            Subscribers = configuration.Subscribers;
            TriggerType = configuration.TriggerType;
            Count = configuration.Count;
            StartPosition = configuration.StartPosition;
            
            TargetMap = configuration.TargetMap;
            TargetMapEntrance = configuration.TargetMapEntrance;
            Delay = configuration.Delay;
        }

        public TriggerConfiguration(Trigger trigger, string ownerGuid = null, bool spawnPrefabOnBuild = true) : base(trigger, ownerGuid, spawnPrefabOnBuild)
        {
            Subscribers = trigger.presetSubscribers.Select(s => s.Guid).ToList();
            TriggerType = trigger.triggerType;
            Count = trigger.count;
            
            if (trigger is IPositionsTrigger positionsTrigger)
            {
                StartPosition = positionsTrigger.GetStartPosition();
            }
            
            if (trigger is MapTraversalTrigger mapTraversalTrigger)
            {
                TargetMap = mapTraversalTrigger.targetMapName;
                TargetMapEntrance = mapTraversalTrigger.targetMapEntryPoint;
                Delay = mapTraversalTrigger.delay;
            }
            
            TransformData.Position = trigger.transform.position.Round(2);
        }
    }
}