using Scripts.InventoryManagement.Inventories;
using Scripts.System.Pooling;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        [SerializeField] private float maxPickupDistance = 1.1f;
        [SerializeField] private float pickupSpawnGracePeriod = 0.2f;
        /// <summary>
        /// Size of image when item is dragged out from inventory
        /// </summary>
        [SerializeField] private Vector2 dragSize = new(100, 100);
        /// <summary>
        /// Offset of ItemCursor while editing an item
        /// </summary>
        [SerializeField] private Vector3 itemEditCursorOffset = new(0, 0.3f, 0);
        [SerializeField] private GameObject pickupColliderPrefab;

        public static float MaxClickPickupDistance { get; private set; }
        public static Vector2 DragSize { get; private set; }
    
        public Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        public static int PickupSpawnGracePeriod { get; private set; }
        public static Vector3 ItemEditCursorOffset => _itemEditCursorOffset;
        
        private static Vector3 _itemEditCursorOffset;

        private void Awake()
        {
            Initialize();
        }
        
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
            if (!Inventory) Initialize();
            
            Inventory.Close();
            Equipment.Close();
        }
        
        public void ClearInventory()
        {
            ActionStore.Clear();
            Inventory.Clear();
            Equipment.Clear();
        }
        
        private void Initialize()
        {
            Inventory = GetComponent<Inventory>();
            ActionStore = GetComponent<ActionStore>();
            Equipment = GetComponent<Equipment>();
            
            MaxClickPickupDistance = maxPickupDistance * maxPickupDistance;
            PickupSpawnGracePeriod = (int) (pickupSpawnGracePeriod * 1000);
            _itemEditCursorOffset = itemEditCursorOffset;
            DragSize = dragSize;
        }
    }
}