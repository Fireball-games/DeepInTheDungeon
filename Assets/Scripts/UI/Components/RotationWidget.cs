using System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;

public class RotationWidget : UIElementBase
{
    private TMP_Text _label;
    private ImageButton _leftRotateButton;
    private ImageButton _rightRotateButton;

    public event Action OnRotateLeft;
    public event Action OnRotateRight;

    private void Awake()
    {
        Transform bodyTransform = body.transform;
        _label = bodyTransform.Find("Label").GetComponent<TMP_Text>();
        _leftRotateButton = bodyTransform.Find("RotateLeft").GetComponent<ImageButton>();
        _rightRotateButton = bodyTransform.Find("RotateRight").GetComponent<ImageButton>();
    }

    private void OnDisable()
    {
        OnRotateLeft = null;
        OnRotateRight  = null;
    }

    public void SetUp(string label, Action onRotateLeft, Action onRotateRight)
    {
        if (_label) _label.text = label ?? "";
        _leftRotateButton.OnClick += onRotateLeft;
        _rightRotateButton.OnClick += onRotateRight;
    }
}
