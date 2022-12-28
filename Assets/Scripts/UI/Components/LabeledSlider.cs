using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class LabeledSlider : UIElementBase
    {
        public Slider slider;
        [SerializeField] private TMP_Text label;

        public UnityEvent<float> OnValueChanged { get; set; } = new();

        private void OnDisable()
        {
            slider.onValueChanged.RemoveAllListeners();
        }

        public float Value
        {
            get => slider.value;
            set
            {
                slider.onValueChanged.RemoveAllListeners();
                slider.value = value;
                slider.onValueChanged.AddListener(OnValueChanged_internal);
            }
        }

        public void SetLabel(string newText) => label.text = newText;

        private void OnValueChanged_internal(float newValue) => OnValueChanged.Invoke(newValue);
    }
}
