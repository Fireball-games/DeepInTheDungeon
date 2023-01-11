using System;
using Scripts.Helpers.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class NumericUpDown : MonoBehaviour
    {
        public float step = 0.1f;
        public float minimum = float.MinValue;
        public float maximum = float.MaxValue;
        [NonSerialized] public TMP_Text Label;
        public bool interactable = true;
        public int precision = 2;
    
        private TMP_InputField _input;
        private Button _plusButton;
        private Button _minusButton;

        private bool _inputUpdateSilent;

        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                _value = SanitizeValue(value);

                if (!_input) Initialize();
                
                _input.onValueChanged.RemoveAllListeners();
                _input.text = value.ToString(GetPrecisionFormatString());
                _input.onValueChanged.AddListener(OnValueChanged_inInput);
            }
        }

        public UnityEvent<float> OnValueChanged { get; set; } = new();
    
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            Label = transform.Find("Label").GetComponent<TMP_Text>();
            _input = transform.Find("Input").GetComponent<TMP_InputField>();
            _input.onValueChanged.RemoveAllListeners();
            _input.onValueChanged.AddListener(OnValueChanged_inInput);
            _plusButton = transform.Find("Buttons").FindActive("PlusButton").GetComponent<Button>();
            _plusButton.onClick.RemoveAllListeners();
            _plusButton.onClick.AddListener(OnPlusClick);
            _minusButton = transform.Find("Buttons").FindActive("MinusButton").GetComponent<Button>();
            _minusButton.onClick.RemoveAllListeners();
            _minusButton.onClick.AddListener(OnMinusClick);
        }

        public void Interactable(bool isInteractable)
        {
            _input.interactable = isInteractable;
            _plusButton.interactable = isInteractable;
            _minusButton.interactable = isInteractable;
            interactable = isInteractable;
        }

        public void SetValue(float newValue, bool isSilent = true) => OnValueChanged_internal(newValue, isSilent);

        private void OnValueChanged_inInput(string newValue)
        {
            if (!float.TryParse(newValue, out float parsedValue)) return;
        
            OnValueChanged_internal(parsedValue);
        }

        private string GetPrecisionFormatString()
        {
            string result = "0.";

            for (int i = 1; i <= precision; i++)
            {
                result += "0";
            }

            return result;
        }

        private void OnValueChanged_internal(float newValue, bool isSilent = false)
        {
            Value = SanitizeValue(newValue);

            if (!isSilent)
            {
                OnValueChanged.Invoke(newValue);
            }
        }

        private void OnPlusClick() => OnValueChanged_internal(Value + step);

        private void OnMinusClick() => OnValueChanged_internal(Value - step);

        private float SanitizeValue(float value) => (float) Math.Round(Mathf.Clamp(value, minimum, maximum), precision);
    }
}
