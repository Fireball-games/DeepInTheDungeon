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
        public override void Set(PrefabBase prefab, UnityAction<PrefabBase> onClick)
        {
            base.Set(prefab, onClick);

            if (prefab.gameObject.GetBody()) AddIcon(EIcon.Wall);
            
            switch (prefab)
            {
                case IMovementWall:
                {
                    AddIcon(EIcon.Move);
                    break;
                }
                case IWallWithTrigger:
                {
                    if (prefab.GetComponent<TriggerReceiver>()) AddIcon(EIcon.TriggerReceiver);
                    if (prefab.GetComponentInChildren<Trigger>()) AddIcon(EIcon.Trigger);
                    break;
                }
            }
        }

        protected override void SetItemName()
        {
            if(!Text) Initialize();
            
            Text.text = displayedItem.gameObject.name;
        }
    }
}