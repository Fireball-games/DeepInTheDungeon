using UnityEngine;

namespace Scripts.Building.PrefabsSpawning
{
    public class EntryPointPrefab : PrefabBase
    {
        public Quaternion lookDirection;

        public override void InitializeFromPool()
        {
            base.InitializeFromPool();
            lookDirection = Quaternion.identity;
        }
    }
}