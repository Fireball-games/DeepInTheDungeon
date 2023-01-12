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

        private UnityEvent<int> OnValueChanged { get; } = new();

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
        }

        public void Set<T>(string labelText, T selectedValue, UnityAction<int> onValueChanged)
        {
            _label.text = string.IsNullOrEmpty(labelText) ? "<missing>" : labelText;

            OnValueChanged.RemoveAllListeners();

            if (typeof(T).IsEnum)
            {
                _options = Enum.GetNames(typeof(T)).ToList();
            }

            if (_options == null || !_options.Any()) return;

            SetOptions(_options);

            _contentType = EContentType.Enum;
            _enumType = typeof(T);

            _dropdown.value = _options.IndexOf(selectedValue.ToString());

            OnValueChanged.AddListener(onValueChanged);
        }

        private void SetOptions(IEnumerable<string> items)
        {
            _dropdown.options.Clear();
            items.ForEach(o => _dropdown.options.Add(new TMP_Dropdown.OptionData(o)));
        }

        private void OnValueChanged_Internal(int value)
        {
            OnValueChanged.Invoke(_contentType is EContentType.Enum ? (int) Enum.Parse(_enumType, _dropdown.options[value].text) : value);
        }
    }
}