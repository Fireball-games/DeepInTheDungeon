using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Scripts.UI.EditorUI.Components
{
    public class EditableConfigurationList : UIElementBase
    {
        [SerializeField] private ConfigurationListButton itemPrefab;
        private Transform _content;
        private Title _title;
        
        private IEnumerable<PrefabConfiguration> _list;

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

            _content.gameObject.DismissAllChildrenToPool(true);
            
            _list.ForEach(item =>
            {
                DeletableConfigurationListButton newButton = ObjectPool.Instance.GetFromPool(itemPrefab.gameObject, _content.gameObject)
                    .GetComponent<DeletableConfigurationListButton>();
                
                newButton.Set(item, null);
            });
        }

        private void OnListChanged_Internal(IEnumerable<PrefabConfiguration> newList)
        {
            if (!Equals(_list, newList)) return;

            OnListChanged.Invoke(newList);
        }
    }
}