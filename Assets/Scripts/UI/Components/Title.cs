using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class Title : UIElementBase
    {
        [SerializeField] private TMP_Text text;

        public void Show(string title)
        {
            SetActive(true);
            text.text = title;
        }

        public void SetTitle(string newTitle) => text.text = newTitle;
    }
}
