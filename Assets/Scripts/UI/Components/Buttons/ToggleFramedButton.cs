﻿using System;
using UnityEngine;

namespace Scripts.UI.Components.Buttons
{
    public class ToggleFramedButton : ImageButton
    {
        [Header("Toggle options")] 
        public bool toggled = true;
        public bool dontToggleOnclick;

        [SerializeField] private Sprite toggleOffSprite;

        public event Action OnToggleOn; 
        public event Action OnToggleOff;

        public void ToggleOn(bool isSilent = false) => SetToggle(true, isSilent);
        public void ToggleOff(bool isSilent = false) => SetToggle(false, isSilent);

        protected override void OnClick_Internal()
        {
            if (!dontToggleOnclick)
            {
                SetToggle(!toggled);
            }
            
            base.OnClick_Internal();
        }

        private void SetToggle(bool isToggled, bool isSilent = false)
        {
            toggled = isToggled;
            
            if (toggled)
            {
                iconImage.sprite = icon;
                if (!isSilent)
                {
                    OnToggleOn?.Invoke();
                }
            }
            else
            {
                iconImage.sprite = toggleOffSprite;
                if (!isSilent)
                {
                    OnToggleOff?.Invoke();
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            iconImage.sprite = toggled ? icon : toggleOffSprite;
        }
#endif
    }
}