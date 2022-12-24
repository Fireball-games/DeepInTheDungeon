using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WaypointControl : MonoBehaviour
{
    private Vector3Control _position;
    private InputField _speedInput;
    private TMP_Text _stepLabel;
    private TMP_Text _stepValue;
    
    private float _step = 1f;

    private void Awake()
    {
        _position = transform.Find("Vector3Control").GetComponent<Vector3Control>();
        _speedInput = transform.Find("SpeedInput").GetComponent<InputField>();

        _stepLabel = transform.Find("AdditionalSettings/StepInfo/StepText").GetComponent<TMP_Text>();
        _stepValue = transform.Find("AdditionalSettings/StepInfo/StepValue").GetComponent<TMP_Text>();
    }

    public void Set(
        string title,
        float step,
        UnityAction<Vector3> onPositionChanged,
        UnityAction<string> onSpeedChanged,
        string xLabel = null, string yLabel = null, string zLabel = null)
    {
        _position.Label.text = title;
        _position.XLabel.text = string.IsNullOrEmpty(xLabel) ? "x" : xLabel;
        _position.YLabel.text = string.IsNullOrEmpty(yLabel) ? "x" : yLabel;
        _position.ZLabel.text = string.IsNullOrEmpty(zLabel) ? "x" : zLabel;
        
        _speedInput.SetTitleText("placeholder");
        _speedInput.OnValueChanged.AddListener(onSpeedChanged);
    }
}
