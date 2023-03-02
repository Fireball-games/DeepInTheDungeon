using System;
using System.Threading.Tasks;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.UI
{
    public class LoadMenu : UIElementBase
    {
        [SerializeField] private GameObject positionRecordPrefab;
        
        private Transform _container;

        private void Awake()
        {
            _container = transform.Find("Background/Frame/ScrollView/Viewport/Content");
        }

        public override async Task SetActiveAsync(bool isActive)
        {
            await base.SetActiveAsync(isActive);

            _container.gameObject.DismissAllChildrenToPool();
            
            if (!isActive) return;
            
            foreach (Save save in SaveManager.Saves)
            {
                PositionRecord positionRecord = ObjectPool.Instance
                    .SpawnFromPool(positionRecordPrefab, _container.gameObject)
                    .GetComponent<PositionRecord>();
                await positionRecord.Set(save);
                await Task.Delay(100);
            }
        }
    }
}