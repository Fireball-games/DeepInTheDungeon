using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine.Events;

namespace Scripts.UI.Components
{
    public class LabeledDropdown : UIElementBase
    {
        private TMP_Text _label;
        private TMP_Dropdown _dropdown;

        private List<string> _options;
        private EContentType _contentType;
        private Type _enumType;

        private UnityEvent<int> OnIntValueChanged { get; } = new();
        private UnityEvent<string> OnStringValueChanged { get; } = new();

        private enum EContentType
        {
            Strings = 1,
            Enum = 2,
        }

        private void Awake()
        {
            _label = body.transform.Find("Label").GetComponent<TMP_Text>();
            _dropdown = body.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(OnValueChanged_Internal);
            
            _options = new List<string>();
            SetOptions(_options);
        }
        
        public void Set(string labelText, IEnumerable<string> options, int selectedValue, UnityAction<string> onValueChanged)
        {
            _label.text = string.IsNullOrEmpty(labelText) ? "<missing>" : labelText;

            OnStringValueChanged.RemoveAllListeners();

            _options = options.ToList();

            if (_options == null || !_options.Any()) return;

            SetOptions(_options);

            _contentType = EContentType.Strings;

            _dropdown.value = selectedValue;

            OnStringValueChanged.AddListener(onValueChanged);
        }

        public void Set<T>(string labelText, T selectedValue, UnityAction<int> onValueChanged)
        {
            _label.text = string.IsNullOrEmpty(labelText) ? "<missing>" : labelText;

            OnIntValueChanged.RemoveAllListeners();

            if (typeof(T).IsEnum)
            {
                _options = Enum.GetNames(typeof(T)).ToList();
            }

            if (_options == null || !_options.Any()) return;

            SetOptions(_options);

            _contentType = EContentType.Enum;
            _enumType = typeof(T);

            _dropdown.value = _options.IndexOf(selectedValue.ToString());

            OnIntValueChanged.AddListener(onValueChanged);
        }

        private void SetOptions(IEnumerable<string> items)
        {
            _dropdown.options.Clear();
            items.ForEach(o => _dropdown.options.Add(new TMP_Dropdown.OptionData(o)));
        }

        private void OnValueChanged(int value)
        {
            OnIntValueChanged.Invoke(value);
        }

        private void OnValueChanged(string value)
        {
            OnStringValueChanged.Invoke(value);
        }

        private void OnValueChanged_Internal(int value)
        {
            switch (_contentType)
            {
                case EContentType.Strings:
                    OnValueChanged(_dropdown.options[value].text);
                    break;
                case EContentType.Enum:
                    OnValueChanged((int) Enum.Parse(_enumType, _dropdown.options[value].text));
                    break;
            }
        }
    }
}