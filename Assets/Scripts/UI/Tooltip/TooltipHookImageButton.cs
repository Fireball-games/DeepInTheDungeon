using System.Collections.Generic;
using Scripts.Localization;
using Scripts.UI.Components;

namespace Scripts.UI.Tooltip
{
    public class TooltipHookImageButton : TooltipHookBase
    {
        private ImageButton _owner;
        private string[] _tooltipStrings;

        protected override void Awake()
        {
            base.Awake();
            
            _owner = GetComponent<ImageButton>();
            _tooltipStrings = new string[1];
        }

        private void Start()
        {
            _tooltipStrings[0] = Keys.GetTooltipText(_owner.gameObject.name);
        }

        protected override IEnumerable<string> GetTooltipStrings() => _tooltipStrings;
    }
}