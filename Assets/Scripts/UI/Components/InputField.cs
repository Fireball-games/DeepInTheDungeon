using Scripts.System.MonoBases;
using TMPro;
using UnityEngine.Events;

namespace Scripts.UI.Components
{
    public class InputField : UIElementBase
    {
        private TMP_InputField _input;
        private TMP_Text _label;
        private TMP_Text _placeholder;

        public string Text => _input.text;

        public UnityEvent<string> OnValueChanged { get; } = new();
        public string PlaceholderText => _placeholder.text;

        private void Awake()
        {
            Initialize();
        }

        public void SetLabelText(string labelText)
        {
            if (!_label) Initialize();
            
            if (string.IsNullOrEmpty(labelText))
            {
                _label.gameObject.SetActive(false);
                return;
            }

            _label.text = labelText;
            _label.gameObject.SetActive(true);
        }

        public void SetPlaceholderText(string newText)
        {
            if (!_placeholder) Initialize();
            
            _placeholder.text = newText;
        }

        public void SetInputText(string newText)
        {
            if (!_input) Initialize();
            
            _input.onValueChanged.RemoveAllListeners();
            _input.text = newText;
            _input.onValueChanged.AddListener(OnValueChanged_internal);
        }

        private void OnValueChanged_internal(string newValue) => OnValueChanged.Invoke(newValue);

        private void Initialize()
        {
            if (_input) return;
            
            _input = body.transform.Find("Frame/Input").GetComponent<TMP_InputField>();
            _label = body.transform.Find("Label").GetComponent<TMP_Text>();
            _placeholder = _input.transform.Find("Text Area/Placeholder").GetComponent<TMP_Text>();
            
            _input.onValueChanged.AddListener(OnValueChanged_internal);
        }
    }
}
