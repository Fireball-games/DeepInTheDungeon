using System;
using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Scripts.UI.EditorUI.Components
{
    public class EditableStringList : UIElementBase
    {
        [SerializeField] private ConfigurationListButton itemPrefab;
        private Transform _content;
        
        private IEnumerable<string> _list;

        private void Awake()
        {
            _list = new List<string>();
        }

        public void Set(IEnumerable<string> items, UnityAction onAddItemClicked, UnityAction<int> onItemDeleted)
        {
            _list = items;
            
            _content.gameObject.DismissAllChildrenToPool(true);
            
            _list.ForEach(item =>
            {
                
            });
        }

        public void Clear()
        {
            _content.gameObject.DismissAllChildrenToPool(true);
        }

        public void Add(Object item)
        {
            
        }

        public void AddRange(IEnumerable<Object> items)
        {
            
        }
    }
}