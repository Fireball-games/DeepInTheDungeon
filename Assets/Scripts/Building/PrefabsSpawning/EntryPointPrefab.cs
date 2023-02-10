using UnityEngine;

namespace Scripts.Building.PrefabsSpawning
{
    public class EntryPointPrefab : PrefabBase
    {
        public bool isMovingForwardOnStart;
        public string entryPointName;
        public Quaternion lookDirection;

        public override void InitializeFromPool()
        {
            base.InitializeFromPool();
            isMovingForwardOnStart = false;
            entryPointName = string.Empty;
            lookDirection = Quaternion.identity;
        }
    }
}