using System;
using System.Threading.Tasks;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using TMPro;
using UnityEngine;

namespace Scripts.UI.MainMenus
{
    public class LoadMenu : UIElementBase
    {
        [SerializeField] private GameObject positionRecordPrefab;

        private TMP_Text _titleText;
        private Transform _container;

        private void Awake()
        {
           AssignComponents();
        }

        public async Task ShowAsync(bool isActive, Action onMapSelected = null)
        {
            if (!_titleText) AssignComponents();
            
            _titleText.text = t.Get(Keys.LoadSavedPosition);
            if (isActive) _container.gameObject.DismissAllChildrenToPool();
            
            SetActive(isActive);
            
            if (!isActive) return;

            foreach (Save save in SaveManager.Saves)
            {
                PositionRecord positionRecord = ObjectPool.Instance
                    .Get(positionRecordPrefab, _container.gameObject)
                    .GetComponent<PositionRecord>();

                await positionRecord.Set(save, () =>
                {
                    onMapSelected?.Invoke();
                    GameManager.Instance.LoadSavedPosition(save);
                });
            }
        }
        
        private void AssignComponents()
        {
            _titleText = transform.Find("Background/Heading/LabelFrame/Title").GetComponent<TMP_Text>();
            _container = transform.Find("Background/Frame/ScrollView/Viewport/Content");
        }
    }
}