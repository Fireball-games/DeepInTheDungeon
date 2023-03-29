using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI.Components.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ButtonSelectedHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Colors.EColor selectedColor;
        [SerializeField] private Colors.EColor unselectedColor;
        
        private Button _button;
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _button.SetTextColor(Colors.Get(selectedColor));
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _button.SetTextColor(Colors.Get(unselectedColor));
        }
    }
}