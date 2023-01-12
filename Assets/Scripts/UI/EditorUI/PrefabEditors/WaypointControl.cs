using System.Globalization;
using Scripts.Localization;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class WaypointControl : MonoBehaviour, IPoolInitializable
    {
        private Vector3Control _position;
        private InputField _speedInput;
        private TMP_Text _stepLabel;
        private TMP_Text _stepValue;
        private Button _deleteButton;

        private UnityEvent<WaypointControl, Vector3> OnPositionChanged { get; } = new();
        private UnityEvent<WaypointControl, float> OnSpeedChanged { get; } = new();

        private UnityEvent<WaypointControl> OnDeleteButtonClicked { get; } = new();

        public float Step
        {
            set
            {
                if (_position)
                {
                    _position.Step = value;
                    _stepValue.text = value.ToString("0.00");
                }
            }
        }

        private void Awake()
        {
            _position = transform.Find("Vector3ControlGridNavigation").GetComponent<Vector3Control>();
            _speedInput = transform.Find("AdditionalSettings/SpeedInput").GetComponent<InputField>();

            _stepLabel = transform.Find("AdditionalSettings/StepLabel").GetComponent<TMP_Text>();
            _stepLabel.text = t.Get(Keys.Step);
        
            _stepValue = transform.Find("AdditionalSettings/StepValue").GetComponent<TMP_Text>();

            _deleteButton = transform.Find("AdditionalSettings/DeleteButtonWrapper/DeleteButton").GetComponent<Button>();
        }

        public void Set(
            string title,
            float step,
            Vector3 position,
            float speed,
            UnityAction<WaypointControl, Vector3> onPositionChanged,
            UnityAction<WaypointControl, float> onSpeedChanged,
            bool isDeleteButtonActive = true,
            UnityAction<WaypointControl> onDeleteButtonClicked = null)
        {
            OnPositionChanged.RemoveAllListeners();
            _position.ValueChanged.RemoveAllListeners();
            _position.Label.text = title;
            _position.Value = position;
            _position.Step = step;
            _position.ValueChanged.AddListener(OnPositionChanged_internal);
            OnPositionChanged.AddListener(onPositionChanged);

            OnSpeedChanged.RemoveAllListeners();
            _speedInput.OnValueChanged.RemoveAllListeners();
            _speedInput.SetTitleText(t.Get(Keys.SpeedTowardsPoint));
            _speedInput.SetInputText(speed.ToString(CultureInfo.InvariantCulture));
            _speedInput.OnValueChanged.AddListener(OnSpeedChanged_internal);
            OnSpeedChanged.AddListener(onSpeedChanged);

            _stepValue.text = step.ToString(CultureInfo.InvariantCulture);
            
            OnDeleteButtonClicked.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
            _deleteButton.gameObject.SetActive(isDeleteButtonActive);

            if (isDeleteButtonActive && onDeleteButtonClicked != null)
            {
                OnDeleteButtonClicked.AddListener(onDeleteButtonClicked);
                _deleteButton.onClick.AddListener(OnDeleteButtonClicked_internal);
            }
        }

        private void OnPositionChanged_internal(Vector3 newPosition)
        {
            OnPositionChanged.Invoke(this, newPosition);
        }
    
        private void OnSpeedChanged_internal(string newSpeed)
        {
            if (float.TryParse(newSpeed, out float parsedValue))
            {
                OnSpeedChanged.Invoke(this, parsedValue);
            }
        }

        private void OnDeleteButtonClicked_internal()
        {
            OnDeleteButtonClicked.Invoke(this);
        }

        public void Initialize()
        {
            OnPositionChanged.RemoveAllListeners();
            OnSpeedChanged.RemoveAllListeners();
        }
    }
}