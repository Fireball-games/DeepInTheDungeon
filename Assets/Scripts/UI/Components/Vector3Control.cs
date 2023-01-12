using System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.UI.Components
{
    public class Vector3Control : UIElementBase
    {
        [SerializeField] private float step;
        [SerializeField] private Vector2 xMinimumMaximum = new(float.MinValue, float.MaxValue);
        [SerializeField] private Vector2 yMinimumMaximum = new(float.MinValue, float.MaxValue);
        [SerializeField] private Vector2 zMinimumMaximum = new(float.MinValue, float.MaxValue);
        public bool interactable = true;

        public TMP_Text Label
        {
            get
            {
                if (!_label) AssignComponents();
                return _label;
            }
        }

        public TMP_Text XLabel
        {
            get
            {
                if (!_xLabel) AssignComponents();
                return _label;
            }
        }
        public TMP_Text YLabel
        {
            get
            {
                if (!_yLabel) AssignComponents();
                return _label;
            }
        }
        
        public TMP_Text ZLabel
        {
            get
            {
                if (!_zLabel) AssignComponents();
                return _label;
            }
        }

        private TMP_Text _label;
        private TMP_Text _xLabel;
        private TMP_Text _yLabel;
        private TMP_Text _zLabel;

        private NumericUpDown _xUpDown;
        private NumericUpDown _yUpDown;
        private NumericUpDown _zUpDown;

        public UnityEvent<Vector3> ValueChanged { get; set; } = new();

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


        public float Step
        {
            get => step;
            set
            {
                step = value;
                _xUpDown.step = value;
                _yUpDown.step = value;
                _zUpDown.step = value;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            AssignComponents();

            _xUpDown.OnValueChanged.AddListener(OnXChanged);
            _xUpDown.minimum = xMinimumMaximum.x;
            _xUpDown.maximum = xMinimumMaximum.y;
            _xUpDown.step = step;
        
            _yUpDown.OnValueChanged.AddListener(OnYChanged);
            _yUpDown.minimum = yMinimumMaximum.x;
            _yUpDown.maximum = yMinimumMaximum.y;
            _yUpDown.step = step;
            
            _zUpDown.OnValueChanged.AddListener(OnZChanged);
            _zUpDown.minimum = zMinimumMaximum.x;
            _zUpDown.maximum = zMinimumMaximum.y;
            _zUpDown.step = step;
        }
        
        public void SetXMinimumMaximum(Vector2 values)
        {
            xMinimumMaximum = values;
            _xUpDown.minimum = values.x;
            _xUpDown.maximum = values.y;
        }

        public void SetYMinimumMaximum(Vector2 values)
        {
            yMinimumMaximum = values;
            _yUpDown.minimum = values.x;
            _yUpDown.maximum = values.y;
        }

        public void SetZMinimumMaximum(Vector2 values)
        {
            zMinimumMaximum = values;
            _zUpDown.minimum = values.x;
            _zUpDown.maximum = values.y;
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
            ValueChanged.Invoke(value);
        }

        private void AssignComponents()
        {
            Transform bodyTransform = body.transform;
            _label = bodyTransform.Find("Label").GetComponent<TMP_Text>();
            _xUpDown = bodyTransform.Find("XUpDown").GetComponent<NumericUpDown>();
            _yUpDown = bodyTransform.Find("YUpDown").GetComponent<NumericUpDown>();
            _zUpDown = bodyTransform.Find("ZUpDown").GetComponent<NumericUpDown>();
            
            _xLabel = _xUpDown.Label;
            _yLabel = _yUpDown.Label;
            _zLabel = _zUpDown.Label;
        }
    }
}