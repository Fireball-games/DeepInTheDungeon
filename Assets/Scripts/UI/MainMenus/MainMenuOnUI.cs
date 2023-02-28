namespace Scripts.UI
{
    public class MainMenuOnUI : MainMenuBase
    {
        public override void SetActive(bool active)
        {
            body.SetActive(active);
            buttonsMenu.SetActive(active);
            loadMenu.SetActive(false);
            startCampaignMenu.SetActive(false);
        }
    }
}