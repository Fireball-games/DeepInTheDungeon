using UnityEngine;

namespace Scripts.Helpers
{
    public class Colors : MonoBehaviour
    {
        [SerializeField] private Color white;
        [SerializeField] private Color deepBlue;
        [SerializeField] private Color lightBlue;
        [SerializeField] private Color positive;
        [SerializeField] private Color negative;
        [SerializeField] private Color warning;
        [SerializeField] private Color selectedOver;
        [SerializeField] private Color selected;
        [SerializeField] private Color disabled;
        [SerializeField] private Color buttonIdle;
        [SerializeField] private Color buttonEntered;
        [SerializeField] private Color buttonClicked;

        public static Color White { get; private set; }
        public static Color Positive { get; private set; }
        public static Color Negative { get; private set; }
        public static Color Warning { get; private set; }
        public static Color Selected { get; private set; }
        public static Color Disabled { get; private set; }
        public static Color SelectedOver { get; private set; }
        public static Color ButtonIdle { get; private set; }
        public static Color ButtonEntered { get; private set; }
        public static Color ButtonClicked { get; private set; }

        public static Color Black => Color.black;
        public static Color Clear => Color.clear;
        public static Color DeepBlue { get; private set; }
        public static Color LightBlue { get; private set; }
        public static Color Yellow { get; } = new(1.0f, 1.0f, 0.0f);
        public static Color Beige { get; } = new(0.96f, 0.96f, 0.86f);
        public static Color FullTransparentBlack { get; } = new(0, 0, 0, 0);
        public static Color Gray { get; } = new(0.5f, 0.5f, 0.5f);

        private void Awake()
        {
            SetColors();
        }

        private void SetColors()
        {
            White = white;
            DeepBlue = deepBlue;
            LightBlue = lightBlue;
            Positive = positive;
            Negative = negative;
            Warning = warning;
            Selected = selected;
            Disabled = disabled;
            SelectedOver = selectedOver;
            ButtonIdle = buttonIdle;
            ButtonEntered = buttonEntered;
            ButtonClicked = buttonClicked;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetColors();
        }
#endif
    }
}