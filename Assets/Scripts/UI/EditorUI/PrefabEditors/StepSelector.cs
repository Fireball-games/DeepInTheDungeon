using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.UI.Components.Buttons;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StepSelector : MonoBehaviour
{
    private TMP_Text _label;
    private TextButton _button005;
    private TextButton _button01;
    private TextButton _button05;
    private TextButton _button1;

    private Dictionary<EStep, ImageButton> _buttonMap;
    private Dictionary<EStep, float> _valueMap;

    public UnityEvent<float> OnStepChanged { get; } = new();

    public enum EStep
    {
        Step005 = 1,
        Step01 = 2,
        Step05 = 3,
        Step1 = 4,
    }

    private void Initialize()
    {
        _label = transform.Find("Label").GetComponent<TMP_Text>();
        _label.text = $"{t.Get(Keys.Step)}:";

        _button005 = transform.Find("Step0.05").GetComponent<TextButton>();
        _button01 = transform.Find("Step0.1").GetComponent<TextButton>();
        _button05 = transform.Find("Step0.5").GetComponent<TextButton>();
        _button1 = transform.Find("Step1").GetComponent<TextButton>();

        _buttonMap = new Dictionary<EStep, ImageButton>
        {
            {EStep.Step005, _button005},
            {EStep.Step01, _button01},
            {EStep.Step05, _button05},
            {EStep.Step1, _button1},
        };
        
        _valueMap = new Dictionary<EStep, float>
        {
            {EStep.Step005, 0.05f},
            {EStep.Step01, 0.1f},
            {EStep.Step05, 0.5f},
            {EStep.Step1, 1f},
        };
    }

    private void OnEnable()
    {
        _button005.OnClickWithSender += OnStepClicked_internal;
        _button01.OnClickWithSender += OnStepClicked_internal;
        _button05.OnClickWithSender += OnStepClicked_internal;
        _button1.OnClickWithSender += OnStepClicked_internal;
    }

    private void OnDisable()
    {
        _button005.OnClickWithSender -= OnStepClicked_internal;
        _button01.OnClickWithSender -= OnStepClicked_internal;
        _button05.OnClickWithSender -= OnStepClicked_internal;
        _button1.OnClickWithSender -= OnStepClicked_internal;
    }

    public void Set(EStep step, UnityAction<float> onStepChanged)
    {
        Initialize();
        
        foreach (EStep key in _buttonMap.Keys)
        {
            ImageButton button = _buttonMap[key];
            button.SetSelected(key == step);
        }
        
        OnStepChanged.RemoveAllListeners();
        OnStepChanged.AddListener(onStepChanged);
    }

    private void OnStepClicked_internal(ImageButton sender)
    {
        OnStepChanged.Invoke(_valueMap[_buttonMap.GetFirstKeyByValue(sender)]);

        foreach (ImageButton button in _buttonMap.Values)
        {
            button.SetSelected(button == sender);
        }
    }
}
