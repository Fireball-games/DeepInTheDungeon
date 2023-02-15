using Scripts.System.MonoBases;
using UnityEngine.UI;

namespace Scripts.UI.PlayMode
{
    public class MainEscapeMenu : EscapeMenuBase
    {
        private GraphicRaycaster _graphicRaycaster;
        
        private void Awake()
        {
            VisibleModal = false;
        }
        
        public void SetGraphicRaycaster(GraphicRaycaster graphicRaycaster)
        {
            _graphicRaycaster = graphicRaycaster;
        }

        protected override void SetContentOnShow()
        {
            _graphicRaycaster.enabled = true;
        }
        
        protected override void SetContentOnClose()
        {
            _graphicRaycaster.enabled = false;
        }
    }
}