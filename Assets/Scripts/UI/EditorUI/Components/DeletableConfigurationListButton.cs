using TMPro;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class DeletableConfigurationListButton : ConfigurationListButton
    {
        protected override void AssignComponents()
        {
            Button = transform.Find("ConfigurationListButton/Button").GetComponent<Button>();
            Text = Button.transform.Find("Text").GetComponent<TMP_Text>();
        }
    }
}