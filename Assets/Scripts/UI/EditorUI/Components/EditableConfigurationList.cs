using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class EditableConfigurationList : UIElementBase
    {
        [SerializeField] private ConfigurationListButton itemPrefab;
        [SerializeField] private Button buttonPrefab;
        private Transform _content;
        private Title _title;

        private IEnumerable<PrefabConfiguration> _list;
        private EditorUIManager UIManager => EditorUIManager.Instance;
        private MapBuilder MapBuilder => GameManager.Instance.MapBuilder;

        private UnityEvent<IEnumerable<PrefabConfiguration>> OnListChanged { get; } = new();

        private void Awake()
        {
            _list = new List<PrefabConfiguration>();
            _content = body.transform.Find("ScrollView/Viewport/Content");
            _title = body.transform.Find("Title").GetComponent<Title>();
        }

        public void Set(string title, IEnumerable<PrefabConfiguration> items, UnityAction<IEnumerable<PrefabConfiguration>> onListChanged)
        {
            _list = items;
            _title.SetTitle(title);
            OnListChanged.RemoveAllListeners();
            OnListChanged.AddListener(onListChanged);

            _content.gameObject.DismissAllChildrenToPool(true);

            _list.ForEach(item =>
            {
                DeletableConfigurationListButton newButton = ObjectPool.Instance.GetFromPool(itemPrefab.gameObject, _content.gameObject)
                    .GetComponent<DeletableConfigurationListButton>();

                newButton.Set(item, null, OnDeleteButtonClicked, false);
            });

            Button addButton = ObjectPool.Instance.GetFromPool(buttonPrefab.gameObject, _content.gameObject)
                .GetComponent<Button>();

            addButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.AddNewSubscriber);
            addButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnAddButtonClicked);
        }

        private void OnAddButtonClicked()
        {
            IEnumerable<TriggerReceiverConfiguration> availableConfigurations = MapBuilder.MapDescription.PrefabConfigurations
                .Where(c => c.PrefabType == Enums.EPrefabType.TriggerReceiver 
                            && !_list.Select(p => p.Guid)
                    .Contains(c.Guid))
                .Select(c => c as TriggerReceiverConfiguration);

            UIManager.SelectConfigurationWindow.Open(t.Get(Keys.SelectSubscriber), availableConfigurations, OnNewSubscriberSelected);
        }

        private void OnDeleteButtonClicked(PrefabConfiguration deletedConfiguration)
        {
            OnListChanged_Internal(_list.Where(c => c.Guid != deletedConfiguration.Guid));
        }

        private void OnNewSubscriberSelected(PrefabConfiguration newReceiver)
        {
            OnListChanged_Internal(_list.Append(newReceiver));
        }

        private void OnListChanged_Internal(IEnumerable<PrefabConfiguration> newList)
        {
            if (Equals(_list, newList)) return;

            OnListChanged.Invoke(newList);
        }
    }
}