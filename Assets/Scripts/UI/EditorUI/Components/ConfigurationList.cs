using Scripts.Building.PrefabsSpawning.Configurations;

namespace Scripts.UI.EditorUI.Components
{
    public class ConfigurationList : ListWindowBase<PrefabConfiguration, ConfigurationListButton>
    {
        protected override string GetItemIdentification(PrefabConfiguration item) => item.Guid;
    }
}