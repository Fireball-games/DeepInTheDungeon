using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class Title : UIElementBase
    {
        [SerializeField] private TMP_Text text;

        public void Show(string title = null)
        {
            bool titleIsNullOrEmpty = string.IsNullOrEmpty(title);
            
            if (titleIsNullOrEmpty)
            {
                SetCollapsed(true);
            }
            else
            {
                SetActive(true);
                text.text = title;
            }
            
        }
        
        public void Hide() => SetActive(false);

        public void SetTitle(string newTitle) => text.text = newTitle;
    }
}
