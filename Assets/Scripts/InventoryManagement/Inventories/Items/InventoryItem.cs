using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Scripts.Building.ItemSpawning;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.InventoryManagement.Inventories.Items
{
    /// <summary>
    /// A ScriptableObject that represents any item that can be put in an
    /// inventory.
    /// </summary>
    /// <remarks>
    /// In practice, you are likely to use a subclass such as `ActionItem` or
    /// `EquipableItem`.
    /// </remarks>
    public abstract class InventoryItem : MapObject
    {
        [Tooltip("Item description to be displayed in UI.")]
        [SerializeField][TextArea] private string description;
        [Tooltip("If true, multiple items of this type can be stacked in the same inventory slot.")]
        [SerializeField] private EUseType useType;
        [SerializeField] private bool stackable;
        [SerializeField, ShowIf(nameof(stackable))] private int maxStackSize;
        [SerializeField] private ItemModifier[] modifiers;
        
        public ItemModifier[] Modifiers => modifiers;
        public EUseType UseType => useType;
        public int MaxStackSize => maxStackSize;

        private static Dictionary<string, InventoryItem> _itemLookupCache;
        
        [Flags]
        public enum EUseType
        {
            None = 1 << 0,
            Equip = 1 << 1,
            Use = 2 << 2,
        }


        /// <summary>
        /// Spawn the pickup GameObject into the world.
        /// </summary>
        /// <param name="position">Where to spawn the pickup.</param>
        /// <param name="number">How many instances of the item does the pickup represent.</param>
        /// <param name="registerInSpawnedMapObjects">If to register spawned item in spawned map objects in ItemSpawner.</param>
        /// <returns>Reference to the pickup object spawned.</returns>
        public Pickup SpawnPickup(Vector3 position, int number, bool registerInSpawnedMapObjects = true)
        {
            Pickup newPickup = pickup.GetFromPool(ItemSpawner.Parent);
            
            if (registerInSpawnedMapObjects)
            {
                ItemSpawner.AddToSpawnedObjects(newPickup.GetInstanceID(), newPickup.gameObject);
            }
            
            newPickup.transform.position = position;
            newPickup.Setup(this, number);
            return newPickup;
        }
        
        public Pickup SpawnPickup(Vector3 position, Quaternion rotation, int number)
        {
            Pickup newPickup = SpawnPickup(position, number);
            newPickup.transform.rotation = rotation;
            return newPickup;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public bool IsStackable()
        {
            return stackable;
        }

        public string GetDescription()
        {
            return description;
        }
    }
}
