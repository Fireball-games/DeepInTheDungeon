using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.Inventory;
using Scripts.Inventory.Inventories.Items;
using Scripts.Localization;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class ItemPreview : PreviewWindowBase
    {
        [SerializeField] private StatText statTextPrefab;
        
        private Transform _statsParent;
        private Title _title;

        public void Show(MapObject item)
        {
            _statsParent.gameObject.DismissAllChildrenToPool();
            
            _title.SetTitle(t.GetItemText(item.DisplayName));

            if (item is InventoryItem inventoryItem)
            {
                ShowStats(inventoryItem.Modifiers);
            }
            
            Show(item.DisplayPrefab, item.DisplayName);
        }

        private void ShowStats(IEnumerable<ItemModifier> modifiers)
        {
            foreach (ItemModifier modifier in modifiers)
            {
                StatText statText = ObjectPool.Instance.Get(statTextPrefab.gameObject, _statsParent.gameObject)
                    .GetComponent<StatText>();
                statText.Set(modifier);
            }
        }

        protected override void AssignReferences()
        {
            base.AssignReferences();
            
            _title = transform.Find("Body/Title").GetComponent<Title>();
            _statsParent = Frame.Find("Stats");
        }
    }
}