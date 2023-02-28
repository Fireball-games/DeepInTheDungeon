using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.UI
{
    public abstract class MainMenuBase : UIElementBase
    {
        protected ButtonsMenu buttonsMenu;
        protected LoadMenu loadMenu;
        protected StartCampaignMenu startCampaignMenu;
        
        protected static GameManager GameManager => GameManager.Instance;
        
        protected void Awake()
        {
            AssignComponents();
        }

        public abstract override void SetActive(bool active);
        
        public void RefreshMainMenuButtons() => buttonsMenu.RefreshButtons();
        
        private void AssignComponents()
        {
            buttonsMenu = GetComponentInChildren<ButtonsMenu>();
            loadMenu = GetComponentInChildren<LoadMenu>();
            startCampaignMenu = GetComponentInChildren<StartCampaignMenu>();
        }
    }
}