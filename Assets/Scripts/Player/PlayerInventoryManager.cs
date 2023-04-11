using System.Collections.Generic;
using Scripts.InventoryManagement.Inventories;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        [SerializeField] private float maxPickupDistance = 1.1f;
        [SerializeField] private float pickupSpawnGracePeriod = 0.2f;
        
        /// <summary>
        /// Offset of ItemCursor while editing an item
        /// </summary>
        [SerializeField] private Vector3 itemEditCursorOffset = new(0, 0.3f, 0);
        [SerializeField] private GameObject pickupColliderPrefab;

        public static float MaxClickPickupDistance { get; private set; }
        public Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        public static int PickupSpawnGracePeriod { get; private set; }
        public static Vector3 ItemEditCursorOffset { get; private set; }

        private void OnEnable()
        {
            pickupColliderPrefab = pickupColliderPrefab.GetFromPool(null);
            pickupColliderPrefab.transform.SetParent(null);
            pickupColliderPrefab.GetComponent<Follow>().target = transform;
        }

        private void OnDisable()
        {
            if (pickupColliderPrefab && ObjectPool.Instance)
            {
                pickupColliderPrefab.gameObject.DismissToPool();
            }
        }
        
        public void SetPickupColliderActive(bool isActive)
        {
            pickupColliderPrefab.SetActive(isActive);
        }

        public void CloseInventories()
        {
            if (!Equipment) Initialize();
            if (!Equipment) return;
            
            Equipment.Close();
        }
        
        public void ClearInventory()
        {
            ActionStore.Clear();
            Inventory.Clear();
            Equipment.Clear();
        }
        
        public void Initialize()
        {
            Equipment = GetComponent<Equipment>();
            if (Equipment) Equipment.Initialize();
            
            Inventory = GetComponent<Inventory>();
            if (Inventory) Inventory.Initialize();

            ActionStore = GetComponent<ActionStore>();
            if (ActionStore) ActionStore.Initialize();
            
            ClearInventory();
            SaveManager.RestoreInventoriesContentFromCurrentSave();
            
            MaxClickPickupDistance = maxPickupDistance * maxPickupDistance;
            PickupSpawnGracePeriod = (int) (pickupSpawnGracePeriod * 1000);
            ItemEditCursorOffset = itemEditCursorOffset;
        }

        public IEnumerable<ISavable> GetInventorySavables() => new ISavable[]{ Inventory, ActionStore, Equipment };
    }
}