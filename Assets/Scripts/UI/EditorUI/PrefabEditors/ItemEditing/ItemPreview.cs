using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.Inventory;
using Scripts.Inventory.Inventories.Items;
using Scripts.Localization;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class ItemPreview : UIElementBase
    {
        [SerializeField] private Texture2D defaultTexture;
        [SerializeField] private StatText statTextPrefab;
        
        private RawImage _previewTargetImage;
        private TMP_Text _previewText;
        private Transform _statsParent;

        private void Awake()
        {
            AssignReferences();

            Hide();
        }
        
        public void Show(MapObject item)
        {
            _statsParent.gameObject.DismissAllChildrenToPool();
            
            _previewTargetImage.texture =
                item.DisplayPrefab 
                    ? Preview3D.Instance.Show(item.DisplayPrefab, Preview3D.EPreviewType.Item) 
                    : defaultTexture;
            
            if (item.DisplayPrefab)
            {
                _previewText.gameObject.SetActive(false);
                _previewText.text = item.DisplayPrefab.name;
            }
            else
            {
                _previewText.text = t.Get(Keys.NoPreviewAvailable);
                _previewText.gameObject.SetActive(true);
            }

            if (item is InventoryItem inventoryItem)
            {
                ShowStats(inventoryItem.Modifiers);
            }

            body.SetActive(true);
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

        public void Hide()
        {
            body.SetActive(false);
            _previewTargetImage.texture = defaultTexture;
            Preview3D.Instance.Hide();
        }
        
        private void AssignReferences()
        {
            Transform frame = transform.Find("Body/Background/Frame");
            _previewTargetImage = frame.Find("PreviewImage").GetComponent<RawImage>();
            _previewText = _previewTargetImage.transform.Find("PreviewText").GetComponent<TMP_Text>();
            _statsParent = frame.Find("Stats");
        }
    }
}