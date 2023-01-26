using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;
        
        private bool _isCollapsed;
        
        /// <summary>
        /// Disables body.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual void SetActive(bool isActive)
        {
            if (!body) return;
            
            body.SetActive(isActive);

            if (isActive && _isCollapsed)
            {
                SetCollapsed(false);
            }
        }

        /// <summary>
        /// Same as SetActive, but disables whole element, not just body.
        /// </summary>
        /// <param name="isCollapsed"></param>
        public void SetCollapsed(bool isCollapsed)
        {
            if (isCollapsed)
            {
                _isCollapsed = true;
                gameObject.SetActive(false);
                SetActive(false);
            }
            else
            {
                _isCollapsed = false;
                gameObject.SetActive(true);
                SetActive(true);
            }
        }

        public bool IsActive => body.activeSelf;
    }
}