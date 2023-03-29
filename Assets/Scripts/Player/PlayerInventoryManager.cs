using Scripts.InventoryManagement.Inventories;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        [SerializeField] private float maxPickupDistance = 1.1f;
        [SerializeField] private float pickupSpawnGracePeriod = 2f;
        [SerializeField] private Vector2 dragSize = new(100, 100);
        [SerializeField] private GameObject pickupColliderPrefab;

        public static float MaxClickPickupDistance { get; private set; }
        public static Vector2 DragSize { get; private set; }
    
        public Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        public static int PickupSpawnGracePeriod { get; private set; }
        public static Vector3 ItemEditCursorOffset => new(0, 0.1f, 0);

        private void Awake()
        {
            Inventory = GetComponent<Inventory>();
            ActionStore = GetComponent<ActionStore>();
            Equipment = GetComponent<Equipment>();
            
            MaxClickPickupDistance = maxPickupDistance * maxPickupDistance;
            PickupSpawnGracePeriod = (int) (pickupSpawnGracePeriod * 1000);
            DragSize = dragSize;
        }
        
        private void OnEnable()
        {
            pickupColliderPrefab = pickupColliderPrefab.GetFromPool(null);
            pickupColliderPrefab.transform.SetParent(null);
            pickupColliderPrefab.GetComponent<Follow>().target = transform;
        }

        private void OnDisable()
        {
            if (pickupColliderPrefab)
            {
                pickupColliderPrefab.gameObject.DismissToPool();
            }
        }

        public void SetPickupColliderActive(bool isActive)
        {
            pickupColliderPrefab.SetActive(isActive);
        }
    }
}