using Scripts.InventoryManagement.Inventories.Items;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// Spawns pickups that should exist on first load in a level. This
    /// automatically spawns the correct prefab for a given inventory item.
    /// </summary>
    public class PickupSpawner : MonoBehaviour, ISavable
    {
        // CONFIG DATA
        [SerializeField] private InventoryItem item;
        [SerializeField] private int number = 1;

        // LIFECYCLE METHODS
        private void Awake()
        {
            // Spawn in Awake so can be destroyed by save system after.
            SpawnPickup();
            Guid = "Pickup Spawner";
        }

        // PUBLIC

        /// <summary>
        /// Returns the pickup spawned by this class if it exists.
        /// </summary>
        /// <returns>Returns null if the pickup has been collected.</returns>
        public Pickup GetPickup() 
        { 
            return GetComponentInChildren<Pickup>();
        }

        /// <summary>
        /// True if the pickup was collected.
        /// </summary>
        public bool IsCollected() 
        { 
            return GetPickup() == null;
        }

        //PRIVATE

        private void SpawnPickup()
        {
            Pickup spawnedPickup = item.SpawnPickup(transform.position, number);
            spawnedPickup.transform.SetParent(transform);
        }

        private void DestroyPickup()
        {
            if (GetPickup())
            {
                Destroy(GetPickup().gameObject);
            }
        }

        public string Guid { get; set; }

        object ISavable.CaptureState()
        {
            return IsCollected();
        }

        void ISavable.RestoreState(object state)
        {
            bool shouldBeCollected = (bool)state;

            if (shouldBeCollected && !IsCollected())
            {
                DestroyPickup();
            }

            if (!shouldBeCollected && IsCollected())
            {
                SpawnPickup();
            }
        }
    }
}