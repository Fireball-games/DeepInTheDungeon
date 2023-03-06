using System.Threading.Tasks;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class LoadMenu : UIElementBase
    {
        [SerializeField] private GameObject positionRecordPrefab;

        private TMP_Text _titleText;
        private Transform _container;

        private void Awake()
        {
            _titleText = transform.Find("Background/Heading/LabelFrame/Title").GetComponent<TMP_Text>();
            _container = transform.Find("Background/Frame/ScrollView/Viewport/Content");
        }

        public override async Task SetActiveAsync(bool isActive)
        {
            _titleText.text = t.Get(Keys.LoadSavedPosition);
            if (isActive) _container.gameObject.DismissAllChildrenToPool();
            
            await base.SetActiveAsync(isActive);
            
            if (!isActive) return;

            foreach (Save save in SaveManager.Saves)
            {
                PositionRecord positionRecord = ObjectPool.Instance
                    .SpawnFromPool(positionRecordPrefab, _container.gameObject)
                    .GetComponent<PositionRecord>();

                await positionRecord.Set(save, () => GameManager.Instance.ContinueFromSave(save));
            }
        }
    }
}