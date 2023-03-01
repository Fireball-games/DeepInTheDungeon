using Scripts.Helpers;
using Scripts.System.MonoBases;
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

        protected override void SetContentOnShow()
        {
            _mainMenu.SetActive(true);
            
            // Logger.Log("Raycaster enabled");
            _graphicRaycaster.enabled = true;
        }
        
        protected override void SetContentOnClose()
        {
            // Logger.Log("Raycaster disabled");
            _graphicRaycaster.enabled = false;
        }
    }
}