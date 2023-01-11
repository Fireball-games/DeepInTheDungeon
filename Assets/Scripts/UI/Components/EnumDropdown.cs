using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine.Events;

namespace Scripts.UI.Components
{
    public class EnumDropdown : UIElementBase
    {
        private TMP_Text _label;
        private TMP_Dropdown _dropdown;

        private IEnumerable<string> _options;

        private UnityEvent<int> OnValueChanged { get; } = new();

        private void Awake()
        {
            _label = body.transform.Find("Label").GetComponent<TMP_Text>();
            _dropdown = body.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(OnValueChanged_Internal);
        }

        public void Set<T>(string labelText, int selectedIndex, UnityAction<int> onValueChanged)
        {
            _label.text = string.IsNullOrEmpty(labelText) ? "<missing>" : labelText;
            
            OnValueChanged.RemoveAllListeners();

            if (typeof(T).IsEnum)
            {
                _options = Enum.GetNames(typeof(T));
            }

            if (_options == null || !_options.Any()) return;
            
            _dropdown.options.Clear();
            _options.ForEach(o => _dropdown.options.Add(new TMP_Dropdown.OptionData(o)));
            
            if (selectedIndex >= 0 && selectedIndex < _options.Count() - 1)
            {
                _dropdown.value = selectedIndex;
            }
            
            OnValueChanged.AddListener(onValueChanged);
        }

        private void OnValueChanged_Internal(int value)
        {
            OnValueChanged.Invoke(value);
        }
    }
}