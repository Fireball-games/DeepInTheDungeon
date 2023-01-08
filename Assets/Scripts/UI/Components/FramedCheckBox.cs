using System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class FramedCheckBox : UIElementBase
    {
        public event Action<bool> OnValueChanged; 

        private Toggle _checkbox;
        private TMP_Text _label;

        private void Awake()
        {
            _checkbox = body.transform.Find("CheckBox").GetComponent<Toggle>();
            _checkbox.onValueChanged.AddListener(OnValueChanged_internal);
            
            _label = _checkbox.transform.Find("Label").GetComponent<TMP_Text>();
        }

        private void OnDisable()
        {
            OnValueChanged = null;
        }

        public bool IsOn => _checkbox.isOn;

        public void SetActive(bool isActive, string label = null, bool isOn = true)
        {
            base.SetActive(isActive);

            if (!isActive) return;
            
            if (!string.IsNullOrEmpty(label)) SetLabel(label);
            
            SetToggle(isOn);
        }
        
        public void SetLabel(string text) => _label.text = text ?? "";

        public void SetToggle(bool isOn)
        {
            _checkbox.onValueChanged.RemoveAllListeners();
            _checkbox.isOn = isOn;
            _checkbox.onValueChanged.AddListener(OnValueChanged_internal);
        }

        private void OnValueChanged_internal(bool value)
        {
            OnValueChanged?.Invoke(value);
        }
    }
}
