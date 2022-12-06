using Scripts.System;
using TMPro;
using UnityEngine;

public class InputField : UIElementBase
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text placeholder;

    public string Text => input.text;
}
