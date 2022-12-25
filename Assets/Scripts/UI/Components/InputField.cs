using System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InputField : UIElementBase
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text placeholder;

    public string Text => input.text;

    public UnityEvent<string> OnValueChanged { get; set; } = new();

    private void Awake()
    {
        input.onValueChanged.AddListener(OnValueChanged_internal);
    }

    public void SetTitleText(string newTitle)
    {
        title.text = newTitle;
    }

    public void SetPlaceholderText(string newText)
    {
        placeholder.text = newText;
    }

    public void SetInputText(string newText)
    {
        input.text = newText;
    }

    private void OnValueChanged_internal(string newValue) => OnValueChanged.Invoke(newValue);
}
