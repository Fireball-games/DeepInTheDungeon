using Scripts.InventoryManagement.Inventories;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        [SerializeField] private float maxPickupDistance = 1.1f;
        [SerializeField] private Vector2 dragSize = new(100, 100);

        public static float MaxClickPickupDistance { get; private set; }
        public static Vector2 DragSize { get; private set; }
    
        public Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        
        private void Awake()
        {
            Inventory = GetComponent<Inventory>();
            ActionStore = GetComponent<ActionStore>();
            Equipment = GetComponent<Equipment>();
            
            MaxClickPickupDistance = maxPickupDistance * maxPickupDistance;
            DragSize = dragSize;
        }
    }
}