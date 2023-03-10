using Scripts.Inventory.Inventories;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Inventory
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
            if (other.gameObject.GetComponent<PlayerController>())
            {
                _pickup.PickupItem();
            }
        }
    }
}