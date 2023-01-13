using Scripts.Localization;
using TMPro;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class SelectConfigurationWindow : ConfigurationList
    {
        private Button _cancelButton;

        protected override void Awake()
        {
            base.Awake();

            _cancelButton = transform.Find("Body/Background/Frame/CancelButtonWrapper/CancelButton").GetComponent<Button>();
            _cancelButton.onClick.AddListener(OnCancelClicked_internal);
            _cancelButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.Cancel);
        }

        protected override void OnCancelClicked_internal()
        {
            base.OnCancelClicked_internal();
            
            SetActive(false);
        }
    }
}