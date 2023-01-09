using Scripts.Building.PrefabsSpawning.Configurations;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationList : ListWindowBase<PrefabConfiguration, NavigatingPrefabListButton>
    {
        protected override void OnItemClicked_internal(PrefabConfiguration item)
        {
            OnItemClicked.Invoke(item);
        }
    }
}