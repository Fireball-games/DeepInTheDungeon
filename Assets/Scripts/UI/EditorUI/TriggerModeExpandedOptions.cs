using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class TriggerModeExpandedOptions : UIElementBase
    {
        private ImageButton _addTriggerButton;
        private ImageButton _editTriggerButton;
        private ImageButton _editTriggerReceiverButton;

        private Dictionary<ETriggerEditMode, ImageButton> _buttonsMap;

        private void Awake()
        {
            _addTriggerButton = body.transform.Find("AddTriggerButton").GetComponent<ImageButton>();
            _editTriggerButton = body.transform.Find("EditTriggerButton").GetComponent<ImageButton>();
            _editTriggerReceiverButton = body.transform.Find("EditTriggerReceiverButton").GetComponent<ImageButton>();
        }

        private void OnEnable()
        {
            _addTriggerButton.OnClickWithSender += OnClick;
            _editTriggerButton.OnClickWithSender += OnClick;
            _editTriggerReceiverButton.OnClickWithSender += OnClick;
            
            EditorEvents.OnTriggerWorkModeChanged += SetSelected;
        }

        private void OnDisable()
        {
            _addTriggerButton.OnClickWithSender -= OnClick;
            _editTriggerButton.OnClickWithSender -= OnClick;
            _editTriggerReceiverButton.OnClickWithSender -= OnClick;
            
            EditorEvents.OnTriggerWorkModeChanged -= SetSelected;
        }

        private void OnClick(ImageButton sender) => MapEditorManager.Instance.SetTriggerEditMode(_buttonsMap.GetFirstKeyByValue(sender));

        public void SetSelected(ETriggerEditMode triggerEditMode)
        {
            _buttonsMap ??= BuildButtonsMap();
            
            foreach (ETriggerEditMode mode in _buttonsMap.Keys)
            {
                _buttonsMap[mode].SetSelected(mode == triggerEditMode);
            }
        }

        private Dictionary<ETriggerEditMode, ImageButton> BuildButtonsMap() =>
            _buttonsMap = new Dictionary<ETriggerEditMode, ImageButton>
            {
                {ETriggerEditMode.AddTrigger, _addTriggerButton},
                {ETriggerEditMode.EditTrigger, _editTriggerButton},
                {ETriggerEditMode.EditReceiver, _editTriggerReceiverButton},
            };
    }
}