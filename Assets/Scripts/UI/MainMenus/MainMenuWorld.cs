namespace Scripts.UI.MainMenus
{
    public class MainMenuWorld : MainMenuBase
    {
        internal override void OpenLoadMenu()
        {
            startCampaignMenu.SetActive(false);
            LoadMenu.Show(true);
        }

        internal override void OpenCustomCampaignMenu()
        {
            LoadMenu.Show(false);
            startCampaignMenu.SetActive(true);
        }
    }
}