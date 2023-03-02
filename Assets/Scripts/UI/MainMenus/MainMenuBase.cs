using System.Threading.Tasks;
using DG.Tweening;
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

        private bool _firstOpenHappened;
        
        protected static GameManager GameManager => GameManager.Instance;
        
        protected void Awake()
        {
            AssignComponents();
        }

        public override async Task SetActiveAsync(bool active)
        {
            if (active)
            {
                body.SetActive(true);
                loadMenu.SetActive(false);
                startCampaignMenu.SetActive(false);
                await buttonsMenu.SetActiveAsync(true);
            }
            else
            {
                Task load = loadMenu.SetActiveAsync(false);
                Task campaign = startCampaignMenu.SetActiveAsync(false);
                Task buttons = buttonsMenu.SetActiveAsync(true);
                
                await Task.WhenAll(load, campaign, buttons);
            }
            
            body.SetActive(active);
        }
        
        protected virtual async void OpenLoadMenu()
        {
            buttonsMenu.transform.DOLocalMove(new Vector3(-500,0, 0), 0.5f);
            await loadMenu.SetActiveAsync(true);
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