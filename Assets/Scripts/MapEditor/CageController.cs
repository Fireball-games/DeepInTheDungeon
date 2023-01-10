using UnityEngine;

namespace Scripts.MapEditor
{
    public class CageController : MonoBehaviour
    {
        [SerializeField] private GameObject cage;
        
        private void OnEnable()
        {
            cage.gameObject.SetActive(false);
        }
        
        public void ShowAt(Vector3 worldPosition, Vector3 scale, Quaternion rotation)
        {
            Transform ownTransform = transform;
            ownTransform.position = worldPosition;
            ownTransform.localRotation = rotation;
            ownTransform.localScale = scale;
            
            cage.SetActive(true);
        }
        
        protected void ShowAt(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            // Logger.Log($"Activating cursor on worldPosition: {worldPosition}");
            cage.SetActive(true);
        }
        
        public virtual void Hide()
        {
            Transform ownTransform = transform;
            ownTransform.localScale = Vector3.one;
            ownTransform.position = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            cage.SetActive(false);
        }
    }
}