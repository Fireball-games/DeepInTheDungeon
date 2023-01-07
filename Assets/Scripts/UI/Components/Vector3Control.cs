using System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.UI.Components
{
    public class Vector3Control : UIElementBase
    {
        public bool interactable = true;
        [NonSerialized] public TMP_Text Label;
        [NonSerialized] public TMP_Text XLabel;
        [NonSerialized] public TMP_Text YLabel;
        [NonSerialized] public TMP_Text ZLabel;

        private NumericUpDown _xUpDown;
        private NumericUpDown _yUpDown;
        private NumericUpDown _zUpDown;

        public UnityEvent<Vector3> OnValueChanged { get; set; } = new();

        private Vector3 _value;

        public Vector3 Value
        {
            get => _value;
            set
            {
                _value = value;
                SetValueExternally();
            }
        }

        private float _step;

        public float Step
        {
            get => _step;
            set
            {
                _step = value;
                _xUpDown.step = value;
                _yUpDown.step = value;
                _zUpDown.step = value;
            }
        }

        public Vector2 XMinimumMaximum
        {
            set
            {
                _xUpDown.minimum = value.x;
                _xUpDown.maximum = value.y;
            }
        }

        public Vector2 YMinimumMaximum
        {
            set
            {
                _yUpDown.minimum = value.x;
                _yUpDown.maximum = value.y;
            }
        }
        
        public Vector2 ZMinimumMaximum
        {
            set
            {
                _zUpDown.minimum = value.x;
                _zUpDown.maximum = value.y;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (Label) return;
            
            Transform bodyTransform = body.transform;
            Label = bodyTransform.Find("Label").GetComponent<TMP_Text>();
        
            _xUpDown = bodyTransform.Find("XUpDown").GetComponent<NumericUpDown>();
            _xUpDown.OnValueChanged.AddListener(OnXChanged);
            XLabel = _xUpDown.transform.Find("Label").GetComponent<TMP_Text>();
        
            _yUpDown = bodyTransform.Find("YUpDown").GetComponent<NumericUpDown>();
            _yUpDown.OnValueChanged.AddListener(OnYChanged);
            YLabel = _yUpDown.transform.Find("Label").GetComponent<TMP_Text>();
        
            _zUpDown = bodyTransform.Find("ZUpDown").GetComponent<NumericUpDown>();
            _zUpDown.OnValueChanged.AddListener(OnZChanged);
            ZLabel = _zUpDown.transform.Find("Label").GetComponent<TMP_Text>();
        }

        private void OnXChanged(float value) => OnValueChanged_internal(new Vector3(value, Value.y, Value.z));
        private void OnYChanged(float value) => OnValueChanged_internal(new Vector3(Value.x, value, Value.z));
        private void OnZChanged(float value) => OnValueChanged_internal(new Vector3(Value.x, Value.y, value));

        private void SetValueExternally()
        {
            _xUpDown.SetValue(_value.x);
            _yUpDown.SetValue(_value.y);
            _zUpDown.SetValue(_value.z);
        }

        private void OnValueChanged_internal(Vector3 value)
        {
            _value = value;
            OnValueChanged.Invoke(value);
        }
    }
}