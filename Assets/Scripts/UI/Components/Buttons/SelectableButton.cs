using NaughtyAttributes;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI.Components.Buttons
{
    public class SelectableButton : Button
    {
        [ReadOnly] public bool selectOnClick = true;
        
        private TMP_Text _text;

        protected override void Awake()
        {
            base.Awake();
            
            _text = GetComponentInChildren<TMP_Text>();
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            
            if (selectOnClick && _text)
            {
                this.SetTextColor(Colors.Selected);
            }
        }
    }
}