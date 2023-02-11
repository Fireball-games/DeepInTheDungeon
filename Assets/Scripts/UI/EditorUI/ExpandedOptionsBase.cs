using System;
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
    public class ExpandedOptionsBase : UIElementBase
    {
        [SerializeField] private EWorkMode DefaultWorkMode;
        [SerializeField] private List<ButtonSetup> buttons;
        public EWorkMode LastSelectedMode { get; private set; }

        
        private Dictionary<EWorkMode, ImageButton> buttonsMap;
        
        public void Awake()
        {
            buttons.ForEach(button => AddButtonToMap(button.button, button.workMode));
            LastSelectedMode = DefaultWorkMode;
        }

        private void OnEnable()
        {
            buttonsMap?.Values.ForEach(b => b.OnClickWithSender += OnClick);
            EditorEvents.OnWorkModeChanged += SetSelected;
            EditorEvents.OnPrefabEdited += OnPrefabEdited;
        }
        
        private void OnDisable()
        {
            buttonsMap?.Values.ForEach(b => b.OnClickWithSender -= OnClick);
            EditorEvents.OnWorkModeChanged -= SetSelected;
            EditorEvents.OnPrefabEdited -= OnPrefabEdited;
        }

        private void AddButtonToMap(ImageButton button, EWorkMode workMode)
        {
            buttonsMap ??= new Dictionary<EWorkMode, ImageButton>();
            buttonsMap.Add(workMode, button);
        }
        
        private void OnPrefabEdited(bool isChanged)
        {
            buttonsMap.Values.ForEach(b => b.SetInteractable(!isChanged));
        }
        
        private void OnClick(ImageButton sender)
        {
            EWorkMode workMode = buttonsMap.GetFirstKeyByValue(sender);
            LastSelectedMode = workMode;
            MapEditorManager.Instance.SetWorkMode(workMode);
        }
        
        public void SetSelected(EWorkMode workMode)
        {
            // this could be a problem if the buttonsMap is not initialized
            //buttonsMap ??= BuildButtonsMap();

            if (buttonsMap.ContainsKey(workMode))
            {
                foreach (EWorkMode mode in buttonsMap.Keys)
                {
                    buttonsMap[mode].SetSelected(mode == workMode);
                }
            }
        }

        [Serializable]
        public class ButtonSetup
        {
            public ImageButton button;
            public EWorkMode workMode;
        }
    }
}