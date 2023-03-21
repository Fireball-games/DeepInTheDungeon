using Scripts.Inventory.Inventories.Items;
using Scripts.Localization;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class ItemPreview : UIElementBase
    {
        [SerializeField] private Texture2D defaultTexture;
        private RawImage _previewTargetImage;
        private TMP_Text _previewText;

        private void Awake()
        {
            AssignReferences();

            Hide();
        }
        
        public void Show(MapObject item)
        {
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
                
            
            body.SetActive(true);
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
        }
    }
}