using Scripts.System;
using Scripts.System.MonoBases;

namespace Scripts.UI
{
    public abstract class MainMenuBase : UIElementBase
    {
        private ButtonsMenu _buttonsMenu;
        private LoadMenu _loadMenu;
        private StartCampaignMenu _startCampaignMenu;
        
        protected static GameManager GameManager => GameManager.Instance;
        
        protected void Awake()
        {
            AssignComponents();
        }

        public abstract override void SetActive(bool active);
        
        public void RefreshMainMenuButtons() => _buttonsMenu.RefreshButtons();
        
        private void AssignComponents()
        {
            _buttonsMenu = GetComponentInChildren<ButtonsMenu>();
            _loadMenu = GetComponentInChildren<LoadMenu>();
            _startCampaignMenu = GetComponentInChildren<StartCampaignMenu>();
        }
    }
}