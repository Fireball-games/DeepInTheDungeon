using UnityEngine;

namespace Scripts.Helpers
{
    public class Colors : MonoBehaviour
    {
        [SerializeField] private Color white;
        [SerializeField] private Color positive;
        [SerializeField] private Color negative;
        [SerializeField] private Color warning;
        [SerializeField] private Color selectedOver;
        [SerializeField] private Color selected;
        [SerializeField] private Color disabled;
        [SerializeField] private Color buttonIdle;
        [SerializeField] private Color buttonEntered;
        [SerializeField] private Color buttonClicked;

        public static Color White => _white;
        public static Color Positive => _positive;
        public static Color Negative => _negative;
        public static Color Warning => _warning;
        public static Color Selected => _selected;
        public static Color Disabled => _disabled;
        public static Color SelectedOver => _selectedOver;
        public static Color ButtonIdle => _buttonIdle;
        public static Color ButtonEntered => _buttonEntered;
        public static Color ButtonClicked => _buttonClicked;
        
        private static Color _white;
        private static Color _positive;
        private static Color _negative;
        private static Color _warning;
        private static Color _selected;
        private static Color _disabled;
        private static Color _selectedOver;
        private static Color _buttonIdle;
        private static Color _buttonEntered;
        private static Color _buttonClicked;

        private void Awake()
        {
            SetColors();
        }

        private void SetColors()
        {
            _white = white;
            _positive = positive;
            _negative = negative;
            _warning = warning;
            _selected = selected;
            _disabled = disabled;
            _selectedOver = selectedOver;
            _buttonIdle = buttonIdle;
            _buttonEntered = buttonEntered;
            _buttonClicked = buttonClicked;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetColors();
        }
#endif
    }
}