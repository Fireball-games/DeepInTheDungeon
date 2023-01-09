using Scripts.Building.PrefabsSpawning.Walls.Identifications;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using Scripts.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static IconStore;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabListButton<T> : ListButtonBase<T> where T : PrefabBase
    {
        [SerializeField] private Image iconPrefab;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Text.gameObject.DismissAllChildrenToPool(true);
        }
        
        public override void Set(T prefab, UnityAction<T> onClick)
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
            
            Text.text = displayedPrefab.gameObject.name;
        }

        private void AddIcon(EIcon icon)
        {
            Image newIcon = ObjectPool.Instance
                .GetFromPool(iconPrefab.gameObject, Text.gameObject, true)
                .GetComponent<Image>();

            newIcon.sprite = Get(icon);
        }
    }
}