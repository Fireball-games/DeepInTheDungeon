namespace Scripts.UI
{
    public class MainMenuWorld : MainMenuBase
    {
        internal override async void OpenLoadMenu()
        {
            await LoadMenu.ShowAsync(true);
        }
    }
}