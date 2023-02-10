using Scripts.UI.Components;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class TriggerModeExpandedOptions : ExtendedOptionsBase
    {
        private ImageButton _editTriggerButton;
        private ImageButton _editTriggerReceiverButton;

        protected override void Awake()
        {
            DefaultWorkMode = EWorkMode.Triggers;
            AddButtonToMap(ref _editTriggerButton, nameof(_editTriggerButton), EWorkMode.Triggers);
            AddButtonToMap(ref _editTriggerReceiverButton, nameof(_editTriggerReceiverButton), EWorkMode.TriggerReceivers);
            
            base.Awake();
        }
    }
}