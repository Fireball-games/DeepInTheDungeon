using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Helpers
{
    public class Colors : MonoBehaviour
    {
        [SerializeField] private Color white;
        [SerializeField] private Color deepBlue;
        [SerializeField] private Color lightBlue;
        [SerializeField] private Color orange;
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
        public static Color Orange = new(1.0f, 0.5f, 0.0f);
        public static Color FullTransparentBlack { get; } = new(0, 0, 0, 0);
        public static Color Gray { get; } = new(0.5f, 0.5f, 0.5f);
        
        public enum EColor
        {
            White = 1,
            DeepBlue = 2,
            LightBlue = 3,
            Orange = 4,
            Positive = 5,
            Negative = 6,
            Warning = 7,
            Selected = 8,
            Disabled = 9,
            SelectedOver = 10,
            ButtonIdle = 11,
            ButtonEntered = 12,
            ButtonClicked = 13,
        }

        private static Dictionary<EColor, Color> _colorsMap;

        private void Awake()
        {
            SetColors();
        }
        
        public static Color GetColor(EColor color) => _colorsMap.ContainsKey(color) ? _colorsMap[color] : White;

        private void SetColors()
        {
            White = white;
            DeepBlue = deepBlue;
            LightBlue = lightBlue;
            Orange = orange;
            Positive = positive;
            Negative = negative;
            Warning = warning;
            Selected = selected;
            Disabled = disabled;
            SelectedOver = selectedOver;
            ButtonIdle = buttonIdle;
            ButtonEntered = buttonEntered;
            ButtonClicked = buttonClicked;
            
            _colorsMap = new Dictionary<EColor, Color>
            {
                {EColor.White, White},
                {EColor.DeepBlue, DeepBlue},
                {EColor.LightBlue, LightBlue},
                {EColor.Orange, Orange},
                {EColor.Positive, Positive},
                {EColor.Negative, Negative},
                {EColor.Warning, Warning},
                {EColor.Selected, Selected},
                {EColor.Disabled, Disabled},
                {EColor.SelectedOver, SelectedOver},
                {EColor.ButtonIdle, ButtonIdle},
                {EColor.ButtonEntered, ButtonEntered},
                {EColor.ButtonClicked, ButtonClicked},
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetColors();
        }
#endif
    }
}