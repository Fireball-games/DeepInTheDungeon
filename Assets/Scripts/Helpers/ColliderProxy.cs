using NaughtyAttributes;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Helpers
{
    public class ColliderProxy : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour target;
        [SerializeField] private bool allInteractions;
        [SerializeField, HideIf(nameof(allInteractions))] private bool useLayerMask;
        [SerializeField, HideIf(nameof(allInteractions)), ShowIf(nameof(useLayerMask))] private LayerMask acceptedLayers;
        [SerializeField, HideIf(nameof(allInteractions))] private bool useTag;
        [SerializeField, HideIf(nameof(allInteractions)), ShowIf(nameof(useTag))] ETag acceptedTag;

        private void OnTriggerEnter(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerStay(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnCollisionEnter", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionExit(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnCollisionExit", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionStay(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessageUpwards("OnCollisionStay", other, SendMessageOptions.DontRequireReceiver);
        }

        private bool ConditionsMet(GameObject other)
        {
            if (allInteractions) return true;
            
            if (!useLayerMask && !useTag) return false;
            
            if (useLayerMask && (acceptedLayers & (1 << other.gameObject.layer)) == 0) return false;

            if (useTag && !other.CompareTag(TagsManager.Get(acceptedTag))) return false;

            return true;
        }
    }
}