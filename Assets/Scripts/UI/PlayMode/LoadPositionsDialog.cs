using Scripts.System.MonoBases;
using Scripts.UI.MainMenus;

namespace Scripts.UI.PlayMode
{
    public class LoadPositionsDialog : DialogBase
    {
        private LoadMenu _loadMenu;

        private void Awake()
        {
            _loadMenu = body.transform.Find("Content/LoadMenu").GetComponent<LoadMenu>();
        }
        
        protected override async void SetContentOnShow()
        {
            await _loadMenu.ShowAsync(true, OnPositionSelected);
        }

        private void OnPositionSelected()
        {
            CloseDialog();
            FindObjectOfType<PlayEscapeMenu>(true).CloseDialog();
        }
    }
}