using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.Triggers;
using UnityEngine.Events;
using static IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabListButton : ListButtonBase<PrefabBase> 
    {
        public override void Set(PrefabBase item, UnityAction<PrefabBase> onClick)
        {
            base.Set(item, onClick);

            if (item.gameObject.GetBody()) AddIcon(EIcon.Wall);
            
            Text.text = displayedItem.gameObject.name;
            
            switch (item)
            {
                case IMovementWall:
                {
                    AddIcon(EIcon.Move);
                    break;
                }
                case IWallWithTrigger:
                {
                    if (item.GetComponent<TriggerReceiver>()) AddIcon(EIcon.TriggerReceiver);
                    if (item.GetComponentInChildren<Trigger>()) AddIcon(EIcon.Trigger);
                    break;
                }
            }
        }
    }
}