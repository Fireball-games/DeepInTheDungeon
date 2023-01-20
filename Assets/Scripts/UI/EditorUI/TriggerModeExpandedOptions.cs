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
        private ImageButton _editTriggerButton;
        private ImageButton _editTriggerReceiverButton;

        private Dictionary<EWorkMode, ImageButton> _buttonsMap;
        public EWorkMode LastSelectedMode { get; private set; }

        private void Awake()
        {
            _editTriggerButton = body.transform.Find("EditTriggerButton").GetComponent<ImageButton>();
            _editTriggerReceiverButton = body.transform.Find("EditTriggerReceiverButton").GetComponent<ImageButton>();
            LastSelectedMode = EWorkMode.Triggers;
        }

        private void OnEnable()
        {
            _editTriggerButton.OnClickWithSender += OnClick;
            _editTriggerReceiverButton.OnClickWithSender += OnClick;
            
            EditorEvents.OnWorkModeChanged += SetSelected;
            EditorEvents.OnPrefabEdited += OnPrefabEdited;
        }

        private void OnDisable()
        {
            _editTriggerButton.OnClickWithSender -= OnClick;
            _editTriggerReceiverButton.OnClickWithSender -= OnClick;
            
            EditorEvents.OnWorkModeChanged -= SetSelected;
            EditorEvents.OnPrefabEdited -= OnPrefabEdited;
        }

        private void OnPrefabEdited(bool isChanged)
        {
            _buttonsMap.Values.ForEach(b => b.SetInteractable(!isChanged));
        }

        private void OnClick(ImageButton sender)
        {
            EWorkMode workMode = _buttonsMap.GetFirstKeyByValue(sender);
            LastSelectedMode = workMode;
            MapEditorManager.Instance.SetWorkMode(workMode);
        }

        public void SetSelected(EWorkMode workMode)
        {
            _buttonsMap ??= BuildButtonsMap();

            if (_buttonsMap.ContainsKey(workMode))
            {
                foreach (EWorkMode mode in _buttonsMap.Keys)
                {
                    _buttonsMap[mode].SetSelected(mode == workMode);
                }
            }
        }

        private Dictionary<EWorkMode, ImageButton> BuildButtonsMap() =>
            _buttonsMap = new Dictionary<EWorkMode, ImageButton>
            {
                {EWorkMode.Triggers, _editTriggerButton},
                {EWorkMode.TriggerReceivers, _editTriggerReceiverButton},
            };
    }
}