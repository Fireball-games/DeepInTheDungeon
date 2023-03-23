using Scripts.Localization;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.Preview3D;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    /// <summary>
    /// Base class for all preview windows. Inheritor needs to implement some kind of "Show" method. This method must call
    /// base Show method with previewObject and previewText parameters. If previewObject is null, defaultTexture will be shown.
    /// </summary>
    public class PreviewWindowBase : UIElementBase
    {
        [SerializeField] protected Texture2D defaultTexture;

        protected Transform Frame;
        
        private RawImage _previewTargetImage;
        private TMP_Text _previewText;
        private Title _title;
        
        private void Awake()
        {
            AssignReferences();

            Hide();
        }

        protected void Show(GameObject previewObject, string previewText = null, EPreviewType previewType = EPreviewType.Tile)
        {
            _title.SetTitle(previewText);
            
            _previewTargetImage.texture =
                previewObject
                    ? SingletonNotPersisting<Preview3D>.Instance.Show(previewObject, previewType) 
                    : defaultTexture;
            
            if (previewObject)
            {
                _previewText.gameObject.SetActive(false);
                _previewText.text = previewText;
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
            SingletonNotPersisting<Preview3D>.Instance.Hide();
        }
    
        protected virtual void AssignReferences()
        {
            Frame = transform.Find("Body/Background/Frame");
            _previewTargetImage = Frame.Find("PreviewImage").GetComponent<RawImage>();
            _previewText = _previewTargetImage.transform.Find("PreviewText").GetComponent<TMP_Text>();
            _title = transform.Find("Body/Title").GetComponent<Title>();
        }
    }
}