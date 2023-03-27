using Scripts.InventoryManagement.Inventories;
using UnityEngine;

namespace Scripts.InventoryManagement
{
    [RequireComponent(typeof(Pickup))]
    public class ClickablePickup : MonoBehaviour
    {
        private Pickup pickup;

        private void Awake()
        {
            pickup = GetComponent<Pickup>();
        }

        private void OnMouseUp()
        {
            if (pickup.CanBePickedUp())
            {
                pickup.PickupItem();
            }
        }

        // public CursorType GetCursorType()
        // {
        //     if (pickup.CanBePickedUp())
        //     {
        //         return CursorType.Pickup;
        //     }
        //     else
        //     {
        //         return CursorType.FullPickup;
        //     }
        // }

        // public bool HandleRaycast(PlayerController callingController)
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         pickup.PickupItem();
        //     }
        //     return true;
        // }
    }
}