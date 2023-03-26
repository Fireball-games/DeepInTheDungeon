using System;
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

        public void SetProxyTarget(MonoBehaviour newTarget) => target = newTarget;

        private void Update()
        {
            if (!target) return;
            
            transform.position = target.transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerStay(Collider other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnCollisionEnter", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionExit(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnCollisionExit", other, SendMessageOptions.DontRequireReceiver);
        }

        private void OnCollisionStay(Collision other)
        {
            if (ConditionsMet(other.gameObject))
                target.SendMessage("OnCollisionStay", other, SendMessageOptions.DontRequireReceiver);
        }

        private bool ConditionsMet(GameObject other)
        {
            if (!target) return false;
            
            if (allInteractions) return true;
            
            if (!useLayerMask && !useTag) return false;
            
            if (useLayerMask && (acceptedLayers & (1 << other.gameObject.layer)) == 0) return false;

            if (useTag && !other.CompareTag(TagsManager.Get(acceptedTag))) return false;

            return true;
        }
    }
}