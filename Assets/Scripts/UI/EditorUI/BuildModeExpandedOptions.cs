using System.Collections.Generic;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.UI.Components;
using UnityEngine;
using static Scripts.MapEditor.Enums;

public class BuildModeExpandedOptions : UIElementBase
{
    [SerializeField] private ImageButton upperLevelButton;
    [SerializeField] private ImageButton sameLevelButton;
    [SerializeField] private ImageButton lowerLevelButton;

    private Dictionary<ELevel, ImageButton> _buttonsMap;

    private void Awake()
    {
        _buttonsMap = new Dictionary<ELevel, ImageButton>
        {
            {ELevel.Equal, sameLevelButton},
            {ELevel.Upper, upperLevelButton},
            {ELevel.Lower, lowerLevelButton},
        };

        upperLevelButton.OnClickWithSender += OnClick;
    }

    private void OnClick(ImageButton sender)
    {
        MapEditorManager.Instance.SetWorkingLevel(_buttonsMap.GetFirstKeyByValue(sender));
    }

    public void SetSelected(ELevel level)
    {
        foreach (ELevel eLevel in _buttonsMap.Keys)
        {
            _buttonsMap[eLevel].SetSelected(eLevel == level);
        }
    }
}
