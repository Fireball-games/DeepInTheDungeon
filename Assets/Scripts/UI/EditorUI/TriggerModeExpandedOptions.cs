using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class TriggerModeExpandedOptions : UIElementBase
    {
        private ImageButton _addTriggerButton;
        private ImageButton _sameLevelButton;
        private ImageButton _lowerLevelButton;

        private Dictionary<ELevel, ImageButton> _buttonsMap;

        private void OnEnable()
        {
            _addTriggerButton.OnClickWithSender += OnClick;
            _sameLevelButton.OnClickWithSender += OnClick;
            _lowerLevelButton.OnClickWithSender += OnClick;

            EditorEvents.OnWorkingLevelChanged += SetSelected;
        }

        private void OnDisable()
        {
            _addTriggerButton.OnClickWithSender -= OnClick;
            _sameLevelButton.OnClickWithSender -= OnClick;
            _lowerLevelButton.OnClickWithSender -= OnClick;

            EditorEvents.OnWorkingLevelChanged -= SetSelected;
        }

        private void OnClick(ImageButton sender) => MapEditorManager.Instance.SetWorkingLevel(_buttonsMap.GetFirstKeyByValue(sender));

        public void SetSelected(ELevel level)
        {
            _buttonsMap ??= BuildButtonsMap();
            
            foreach (ELevel eLevel in _buttonsMap.Keys)
            {
                _buttonsMap[eLevel].SetSelected(eLevel == level);
            }
        }

        private Dictionary<ELevel, ImageButton> BuildButtonsMap() =>
            _buttonsMap = new Dictionary<ELevel, ImageButton>
            {
                {ELevel.Equal, _sameLevelButton},
                {ELevel.Upper, _addTriggerButton},
                {ELevel.Lower, _lowerLevelButton},
            };
    }
}