using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabeledSlider : UIElementBase
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    public float Value
    {
        get => slider.value;
        set => slider.value = value;
    }

    public void SetLabel(string newText) => label.text = newText;
}
