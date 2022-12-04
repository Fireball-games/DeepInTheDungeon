using TMPro;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject body;

        public void SetActive(bool isActive) => body.SetActive(isActive);

        public void Show(string title)
        {
            SetActive(true);
            titleText.text = title;
        }
    }
}
