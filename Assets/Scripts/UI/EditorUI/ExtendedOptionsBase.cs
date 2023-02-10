﻿using System;
using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public abstract class ExtendedOptionsBase : UIElementBase
    {
        protected Dictionary<EWorkMode, ImageButton> buttonsMap;
        public EWorkMode LastSelectedMode { get; set; }
        protected EWorkMode DefaultWorkMode;
        
        /// <summary>
        /// Example of awake Method:
        /// <code>
        /// private void Awake()
        /// {
        ///     DefaultWorkMode = EWorkMode.Triggers;
        ///     AddButton(_editTriggerButton, EWorkMode.Triggers);
        ///     AddButton(_editTriggerReceiverButton, EWorkMode.TriggerReceivers);
        ///
        ///     base.Awake();
        /// }
        /// </code>
        /// </summary>
        protected virtual void Awake()
        {
            LastSelectedMode = EWorkMode.Triggers;
        }

        private void OnEnable()
        {
            buttonsMap.Values.ForEach(b => b.OnClickWithSender += OnClick);
            EditorEvents.OnWorkModeChanged += SetSelected;
            EditorEvents.OnPrefabEdited += OnPrefabEdited;
        }
        
        private void OnDisable()
        {
            buttonsMap.Values.ForEach(b => b.OnClickWithSender -= OnClick);
            EditorEvents.OnWorkModeChanged -= SetSelected;
            EditorEvents.OnPrefabEdited -= OnPrefabEdited;
        }

        protected void AddButtonToMap(ref ImageButton button, string buttonName, EWorkMode workMode)
        {
            button = body.transform.Find(GetObjectNameFromVariableName(buttonName)).GetComponent<ImageButton>();
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
        
        string GetObjectNameFromVariableName(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2)
            {
                return input;
            }

            string modifiedString = input.Substring(1);
            modifiedString = char.ToUpper(modifiedString[0]) + modifiedString.Substring(1);
            return modifiedString;
        }
    }
}