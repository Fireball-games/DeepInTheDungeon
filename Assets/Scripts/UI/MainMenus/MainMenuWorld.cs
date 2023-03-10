namespace Scripts.UI.MainMenus
{
    public class MainMenuWorld : MainMenuBase
    {
        internal override async void OpenLoadMenu()
        {
            await startCampaignMenu.SetActiveAsync(false);
            await LoadMenu.ShowAsync(true);
        }

        internal override async void OpenCustomCampaignMenu()
        {
            await LoadMenu.ShowAsync(false);
            await startCampaignMenu.SetActiveAsync(true);
        }
    }
}