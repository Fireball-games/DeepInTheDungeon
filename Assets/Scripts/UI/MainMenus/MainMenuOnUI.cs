using NotImplementedException = System.NotImplementedException;

namespace Scripts.UI
{
    public class MainMenuOnUI : MainMenuBase
    {
        public override void SetActive(bool active) => body.SetActive(active);
    }
}