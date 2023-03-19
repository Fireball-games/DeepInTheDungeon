using System.Collections.Generic;
using Scripts.Localization;

namespace Scripts.UI.Tooltip
{
    public class TooltipHookSimpleTitle : TooltipHookBase
    {
        private string[] _tooltipStrings;

        protected override void Awake()
        {
            base.Awake();
            
            _tooltipStrings = new string[1];
        }

        private void Start()
        {
            _tooltipStrings[0] = t.GetTooltipText(OwnerTransform.gameObject.name);
        }

        protected override IEnumerable<string> GetTooltipStrings() => _tooltipStrings;
    }
}