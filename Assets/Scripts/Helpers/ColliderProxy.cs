using UnityEngine;

namespace Scripts.Helpers
{
    public class ColliderProxy : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour target;
        
        private void OnTriggerEnter(Collider other) => target.SendMessageUpwards("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        private void OnTriggerExit(Collider other) => target.SendMessageUpwards("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        private void OnTriggerStay(Collider other) => target.SendMessageUpwards("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver);
        private void OnCollisionEnter(Collision other) => target.SendMessageUpwards("OnCollisionEnter", other, SendMessageOptions.DontRequireReceiver);
        private void OnCollisionExit(Collision other) => target.SendMessageUpwards("OnCollisionExit", other, SendMessageOptions.DontRequireReceiver);
        private void OnCollisionStay(Collision other) => target.SendMessageUpwards("OnCollisionStay", other, SendMessageOptions.DontRequireReceiver);
    }
}