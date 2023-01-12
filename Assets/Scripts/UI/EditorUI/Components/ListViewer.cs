using System;
using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.UI.EditorUI.Components
{
    public class ListViewer : UIElementBase
    {
        [SerializeField] private ListButtonBase<ConfigurationListButton> itemPrefab;
        private Transform _content;
        
        private List<Object> _list;

        private void Awake()
        {
            _list = new List<Object>();
        }

        public void Clear()
        {
            _content.gameObject.DismissAllChildrenToPool(true);
            _list.Clear();
        }

        public void Add(Object item)
        {
            
        }

        public void AddRange(IEnumerable<Object> items)
        {
            
        }
    }
}