using Scripts.Helpers;
using Scripts.InventoryManagement.Inventories;
using UnityEngine;

namespace Scripts.InventoryManagement
{
    [RequireComponent(typeof(Pickup))]
    public class ProximityPickup : MonoBehaviour
    {
        private Pickup _pickup;
        
        private void Awake()
        {
            _pickup = GetComponent<Pickup>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(TagsManager.PickupCollider))
            {
                _pickup.PickupItem();
            }
        }
    }
}