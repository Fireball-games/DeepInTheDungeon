using NotImplementedException = System.NotImplementedException;

namespace Scripts.UI
{
    public class MainMenuWorld : MainMenuBase
    {
        public override void SetActive(bool active) => body.SetActive(active);
    }
}