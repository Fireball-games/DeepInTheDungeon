using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using UnityEngine.Events;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationList : ListWindowBase<PrefabConfiguration, ConfigurationListButton>
    {
        public bool isCameraStayingOnNavigatedPosition;

        public void Open(string listTitle,
            IEnumerable<PrefabConfiguration> items,
            UnityAction<PrefabConfiguration> onItemClicked,
            bool _isCameraStayingOnNavigatedPosition,
            UnityAction onClose = null)
        {
            base.Open(listTitle, items, onItemClicked, onClose);

            if (LastAddedButton)
            {
                LastAddedButton.isCameraStayingOnNavigatedPosition = _isCameraStayingOnNavigatedPosition;
            }
            isCameraStayingOnNavigatedPosition = _isCameraStayingOnNavigatedPosition;
        }

        protected override void SetButton(ConfigurationListButton button, PrefabConfiguration item)
        {
            button.Set(item, OnItemClicked_internal, SetClickedItemSelected, isCameraStayingOnNavigatedPosition);
        }

        protected override string GetItemIdentification(PrefabConfiguration item) => item.Guid;
    }
}