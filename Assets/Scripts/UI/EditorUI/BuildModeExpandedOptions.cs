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
    public class BuildModeExpandedOptions : UIElementBase
    {
        [SerializeField] private ImageButton upperLevelButton;
        [SerializeField] private ImageButton sameLevelButton;
        [SerializeField] private ImageButton lowerLevelButton;

        private Dictionary<ELevel, ImageButton> _buttonsMap;

        private void OnEnable()
        {
            upperLevelButton.OnClickWithSender += OnClick;
            sameLevelButton.OnClickWithSender += OnClick;
            lowerLevelButton.OnClickWithSender += OnClick;

            EditorEvents.OnWorkingLevelChanged += SetSelected;
        }

        private void OnDisable()
        {
            upperLevelButton.OnClickWithSender -= OnClick;
            sameLevelButton.OnClickWithSender -= OnClick;
            lowerLevelButton.OnClickWithSender -= OnClick;

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
                {ELevel.Equal, sameLevelButton},
                {ELevel.Upper, upperLevelButton},
                {ELevel.Lower, lowerLevelButton},
            };
    }
}
