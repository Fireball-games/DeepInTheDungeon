using System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class RotationWidget : UIElementBase
    {
        private TMP_Text _label;
        private ImageButton _leftRotateButton;
        private ImageButton _rightRotateButton;

        private void Awake()
        {
            Transform bodyTransform = body.transform;
            _label = bodyTransform.Find("Label").GetComponent<TMP_Text>();
            _leftRotateButton = bodyTransform.Find("RotateLeft").GetComponent<ImageButton>();
            _rightRotateButton = bodyTransform.Find("RotateRight").GetComponent<ImageButton>();
        }

        public void SetUp(string label, Action onRotateLeft, Action onRotateRight)
        {
            SetActive(true);
        
            if (_label) _label.text = label ?? "";
            _leftRotateButton.OnClick += onRotateLeft;
            _rightRotateButton.OnClick += onRotateRight;
        
        }
    }
}
