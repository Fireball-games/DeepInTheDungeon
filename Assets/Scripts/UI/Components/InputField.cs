using Scripts.System;
using TMPro;
using UnityEngine;

public class InputField : UIElementBase
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text placeholder;

    public string Text => input.text;

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
}
