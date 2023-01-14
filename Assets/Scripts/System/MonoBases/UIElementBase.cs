using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;

        private Vector3 _originalScale;

        public virtual void SetActive(bool isActive)
        {
            if (isActive)
            {
                if (transform.localScale != Vector3.zero)
                {
                    _originalScale = transform.localScale;
                }
                else
                {
                    SetCollapsed(false);
                }
            }
            
            if (body)
            {
                body.SetActive(isActive);
            }
        }

        public void Reparent(Transform newParent, bool isActive = true)
        {
            transform.SetParent(newParent);
            SetActive(isActive);
        }

        public void SetCollapsed(bool isCollapsed)
        {
            if (isCollapsed)
            {
                if (transform.localScale == Vector3.zero) return;
                
                _originalScale = transform.localScale;
                transform.localScale = Vector3.zero;
                SetActive(false);
            }
            else
            {
                if (transform.localScale == _originalScale) return;
                
                transform.localScale = _originalScale;
                SetActive(true);
            }
        }

        public bool IsActive => body.activeSelf;
    }
}