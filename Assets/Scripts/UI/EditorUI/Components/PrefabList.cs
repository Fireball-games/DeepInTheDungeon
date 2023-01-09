using Scripts.Building.Walls;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabList : ListWindowBase<PrefabBase, PrefabListButton>
    {
        protected override void OnItemClicked_internal(PrefabBase item)
        {
            string prefabName = item.gameObject.name;

            OnItemClicked.Invoke(item);

            foreach (PrefabListButton button in _buttons)
            {
                if (button.displayedItem.gameObject.name != prefabName)
                {
                    button.SetSelected(false);
                }
            }
        }
    }
}