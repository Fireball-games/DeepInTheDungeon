using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Logger = Scripts.Helpers.Logger;

public class Vector3Control : MonoBehaviour
{
    [NonSerialized] public TMP_Text Label;
    [NonSerialized] public TMP_Text XLabel;
    [NonSerialized] public TMP_Text YLabel;
    [NonSerialized] public TMP_Text ZLabel;
    public bool interactable = true;

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
            SetNewValue();
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

    private void SetNewValue()
    {
        _xUpDown.SetValue(_value.x);
        _yUpDown.SetValue(_value.y);
        _zUpDown.SetValue(_value.z);
    }
}