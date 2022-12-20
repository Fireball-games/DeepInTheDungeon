using Scripts.System.MonoBases;
using TMPro;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class FramedCheckBox : UIElementBase
    {
        private Toggle _checkbox;
        private TMP_Text _label;

        private void Awake()
        {
            _checkbox = body.transform.Find("CheckBox").GetComponent<Toggle>();
            _label = _checkbox.transform.Find("Label").GetComponent<TMP_Text>();
        }

        public bool IsOn => _checkbox.isOn;

        public void SetLabel(string text) => _label.text = text ?? "";

        public void SetToggle(bool isOn) => _checkbox.isOn = isOn;
    }
}
