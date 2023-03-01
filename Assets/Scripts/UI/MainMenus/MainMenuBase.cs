using System.Threading.Tasks;
using Scripts.System;
using Scripts.System.MonoBases;

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

        public override async Task SetActiveAsync(bool active)
        {
            body.SetActive(true);
            Task buttons = buttonsMenu.SetActiveAsync(active);
            Task load = loadMenu.SetActiveAsync(false);
            Task campaign = startCampaignMenu.SetActiveAsync(false);
            
            await Task.WhenAll(buttons, load, campaign);
            body.SetActive(active);
        }
        
        public void RefreshMainMenuButtons() => buttonsMenu.RefreshButtons();
        
        private void AssignComponents()
        {
            buttonsMenu = GetComponentInChildren<ButtonsMenu>(true);
            loadMenu = GetComponentInChildren<LoadMenu>(true);
            startCampaignMenu = GetComponentInChildren<StartCampaignMenu>(true);
        }
    }
}