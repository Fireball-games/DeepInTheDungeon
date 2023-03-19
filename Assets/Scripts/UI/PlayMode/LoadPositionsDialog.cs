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
        
        protected override void SetContentOnShow()
        {
            _loadMenu.Show(true, OnPositionSelected);
        }

        private void OnPositionSelected()
        {
            CloseDialog();
            FindObjectOfType<PlayEscapeMenu>(true).CloseDialog();
        }
    }
}