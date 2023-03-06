using System.Threading.Tasks;
using Scripts.System.MonoBases;

namespace Scripts.UI
{
    public abstract class MainMenuBase : UIElementBase
    {
        private ButtonsMenu _buttonsMenu;
        protected LoadMenu LoadMenu;
        protected StartCampaignMenu startCampaignMenu;
        
        private MainUIManager UIManager => MainUIManager.Instance;

        protected void Awake()
        {
            AssignComponents();
            
            _buttonsMenu.PrepareForTransition();
            LoadMenu.PrepareForTransition();
            startCampaignMenu.PrepareForTransition();
        }

        public override async Task SetActiveAsync(bool active)
        {
            if (active)
            {
                body.SetActive(true);
                LoadMenu.SetActive(false);
                startCampaignMenu.SetActive(false);
                await _buttonsMenu.SetActiveAsync(true);
            }
            else
            {
                Task load = LoadMenu.SetActiveAsync(false);
                Task campaign = startCampaignMenu.SetActiveAsync(false);
                Task buttons = _buttonsMenu.SetActiveAsync(false);
                
                await Task.WhenAll(load, campaign, buttons);
            }
            
            CanvasGroup.blocksRaycasts = active;
            body.SetActive(active);
        }
        
        internal abstract void OpenLoadMenu();

        public void RefreshMainMenuButtons() => _buttonsMenu.SetComponents();
        
        private void AssignComponents()
        {
            _buttonsMenu = GetComponentInChildren<ButtonsMenu>(true);
            LoadMenu = GetComponentInChildren<LoadMenu>(true);
            startCampaignMenu = GetComponentInChildren<StartCampaignMenu>(true);
        }
    }
}