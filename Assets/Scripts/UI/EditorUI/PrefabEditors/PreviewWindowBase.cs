using Scripts.Localization;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        
        private void Awake()
        {
            AssignReferences();

            Hide();
        }

        protected void Show(GameObject previewObject, string previewText = null)
        {
            _previewTargetImage.texture =
                previewObject
                    ? Preview3D.Instance.Show(previewObject, Preview3D.EPreviewType.Item) 
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
            Preview3D.Instance.Hide();
        }
    
        protected virtual void AssignReferences()
        {
            Frame = transform.Find("Body/Background/Frame");
            _previewTargetImage = Frame.Find("PreviewImage").GetComponent<RawImage>();
            _previewText = _previewTargetImage.transform.Find("PreviewText").GetComponent<TMP_Text>();
        }
    }
}