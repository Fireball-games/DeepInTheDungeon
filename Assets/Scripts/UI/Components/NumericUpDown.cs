using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumericUpDown : MonoBehaviour
{
    public float value;
    public float step = 0.1f;
    public float minimum = float.MinValue;
    public float maximum = float.MaxValue;
    [NonSerialized] public TMP_Text label;
    public bool interactable = true;
    
    private TMP_InputField _input;
    private Button _plusButton;
    private Button _minusButton;

    public UnityEvent<float> OnValueChanged { get; set; } = new();

    private void Awake()
    {
        label = transform.Find("Label").GetComponent<TMP_Text>();
        _input = transform.Find("Input").GetComponent<TMP_InputField>();
        _input.onValueChanged.AddListener(OnValueChanged_inInput);
        _plusButton = transform.Find("Buttons/PlusButton").GetComponent<Button>();
        _plusButton.onClick.AddListener(OnPlusClick);
        _minusButton = transform.Find("Buttons/MinusButton").GetComponent<Button>();
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
        if (float.TryParse(newValue, out float parsedValue))
        {
            OnValueChanged_internal(parsedValue);
            return;
        }

        _input.text = value.ToString(CultureInfo.InvariantCulture);
    }

    private void OnValueChanged_internal(float newValue, bool isSilent = false)
    {
        value = Mathf.Clamp(newValue, minimum, maximum);
        _input.text = value.ToString(CultureInfo.InvariantCulture);
        
        if (!isSilent)
        {
            OnValueChanged.Invoke(newValue);
        }
    }

    private void OnPlusClick() => OnValueChanged_internal(value + step);

    private void OnMinusClick() => OnValueChanged_internal(value - step);
}
