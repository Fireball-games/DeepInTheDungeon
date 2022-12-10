using Scripts.UI.Components;
using TMPro;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class FloorButton : ImageButton
    {
        [SerializeField] private TMP_Text textField;

        public void SetActive(bool isActive, string text)
        {
            base.SetActive(isActive);

            textField.text = text;
        }
    }
}