using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.Localization;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class ItemPreview : PreviewWindowBase
    {
        [SerializeField] private StatText statTextPrefab;
        
        private Transform _statsParent;

        public void Show(MapObject item)
        {
            _statsParent.gameObject.DismissAllChildrenToPool();

            if (item is InventoryItem inventoryItem)
            {
                ShowStats(inventoryItem.Modifiers);
            }
            
            Show(item.DisplayPrefab, t.GetItemText(item.DisplayName), Preview3D.EPreviewType.Item);
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
            
            _statsParent = Frame.Find("Stats");
        }
    }
}