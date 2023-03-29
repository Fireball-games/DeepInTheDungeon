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
        [SerializeField] private Color lightGray;
        [SerializeField] private Color lightRed;
        [SerializeField] private Color darkRed;
        [SerializeField] private Color lightYellow;
        [SerializeField] private Color darkGreen;

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
        public static Color LightGray { get; private set; }
        public static Color LightRed { get; private set; }
        public static Color DarkRed { get; private set; }
        public static Color LightYellow { get; private set; }
        public static Color DarkGreen { get; private set; }
        public static Color LightBlue { get; private set; }
        public static Color DeepBlue { get; private set; }

        public static Color Black => Color.black;
        public static Color Clear => Color.clear;
        public static Color Yellow { get; } = new(1.0f, 1.0f, 0.0f);
        public static Color Beige { get; } = new(0.96f, 0.96f, 0.86f);
        public static Color Orange = new(1.0f, 0.5f, 0.0f);
        public static Color FullTransparentBlack { get; } = new(0, 0, 0, 0);
        public static Color FullOpaqueWhite { get; } = new(1, 1, 1, 1);
        public static Color Gray { get; } = new(0.5f, 0.5f, 0.5f);
        
        public enum EColor
        {
            White = 1,
            LightBlue = 3,
            DeepBlue = 2,
            Orange = 4,
            LightGray = 14,
            LightRed = 15,
            DarkRed = 16,
            LightYellow = 17,
            DarkGreen = 18,
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
        
        public static Color Get(EColor color) => _colorsMap.ContainsKey(color) ? _colorsMap[color] : White;

        private void SetColors()
        {
            White = white;
            DeepBlue = deepBlue;
            LightBlue = lightBlue;
            LightGray = lightGray;
            LightRed = lightRed;
            DarkRed = darkRed;
            LightYellow = lightYellow;
            DarkGreen = darkGreen;
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
                {EColor.LightGray, LightGray},
                {EColor.LightRed, LightRed},
                {EColor.DarkRed, DarkRed},
                {EColor.LightYellow, LightYellow},
                {EColor.DarkGreen, DarkGreen},
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