using TMPro;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private GameObject body;

        public void SetActive(bool isActive) => body.SetActive(isActive);

        public void Show(string title)
        {
            SetActive(true);
            text.text = title;
        }

        public void SetTitle(string newTitle) => text.text = newTitle;
    }
}
