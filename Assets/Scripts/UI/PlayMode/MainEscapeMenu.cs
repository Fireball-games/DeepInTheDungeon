using Scripts.System.MonoBases;

namespace Scripts.UI.PlayMode
{
    public class MainEscapeMenu : EscapeMenuBase
    {
        private void Awake()
        {
            VisibleModal = false;
            CancelOnModalClick = false;
            CancelOnEscape = false;
        }

        protected override void SetContentOnShow()
        {
        }
    }
}