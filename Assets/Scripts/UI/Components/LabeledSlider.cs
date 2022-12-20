using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabeledSlider : UIElementBase
{
    public Slider slider;
    [SerializeField] private TMP_Text label;

    private void OnDisable()
    {
        slider.onValueChanged.RemoveAllListeners();
    }

    public float Value
    {
        get => slider.value;
        set => slider.value = value;
    }

    public void SetLabel(string newText) => label.text = newText;
}
