using System.Threading.Tasks;
using Scripts.System.MonoBases;
using Scripts.UI.MainMenus;
using UnityEngine.UI;

namespace Scripts.UI.PlayMode
{
    public class MainEscapeMenu : EscapeMenuBase
    {
        private GraphicRaycaster _graphicRaycaster;
        private MainMenuOnUI _mainMenu;
        
        private void Awake()
        {
            VisibleModal = false;
            _mainMenu = GetComponentInChildren<MainMenuOnUI>();
        }
        
        public void SetGraphicRaycaster(GraphicRaycaster graphicRaycaster)
        {
            _graphicRaycaster = graphicRaycaster;
        }

        protected override async void SetContentOnShow()
        {
            await _mainMenu.SetActiveAsync(true);
            
            // Logger.Log("Raycaster enabled");
            _graphicRaycaster.enabled = true;
        }
        
        protected override async Task SetContentOnClose()
        {
            await _mainMenu.SetActiveAsync(false);
            // Logger.Log("Raycaster disabled");
            _graphicRaycaster.enabled = false;
        }
    }
}