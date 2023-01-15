using Scripts.Building.PrefabsSpawning.Configurations;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.Components
{
    public class DeletableConfigurationListButton : ConfigurationListButton
    {
        private Button _deleteButton;

        private UnityEvent<PrefabConfiguration> OnDeleteClicked { get; } = new();

        protected override void AssignComponents()
        {
            Button = transform.Find("ConfigurationListButton/Button").GetComponent<Button>();
            Text = Button.transform.Find("Text").GetComponent<TMP_Text>();
            _deleteButton = transform.Find("DeleteButton").GetComponent<Button>();
            _deleteButton.onClick.AddListener(OnDeleteClicked_Internal);
        }

        public void Set(PrefabConfiguration item, UnityAction<PrefabConfiguration> onClick, UnityAction<PrefabConfiguration> onDeleteItemClick, bool setSelectedOnClick)
        {
            base.Set(item, onClick, setSelectedOnClick);
            
            OnDeleteClicked.RemoveAllListeners();
            OnDeleteClicked.AddListener(onDeleteItemClick);
        }

        private void OnDeleteClicked_Internal()
        {
            OnDeleteClicked.Invoke(displayedItem);
        }
    }
}