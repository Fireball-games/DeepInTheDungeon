using Scripts.Helpers;
using Scripts.InventoryManagement.Inventories;
using UnityEngine;

namespace Scripts.InventoryManagement
{
    [RequireComponent(typeof(Pickup))]
    public class ProximityPickup : MonoBehaviour
    {
        private Pickup _pickup;
        private bool _isPickedUp;
        
        private void Awake()
        {
            _pickup = GetComponent<Pickup>();
        }

        private void OnEnable()
        {
            _isPickedUp = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPickedUp || !other.gameObject.CompareTag(TagsManager.PickupCollider)) return;
            
            _pickup.PickupItem();
            _isPickedUp = true;
        }
    }
}