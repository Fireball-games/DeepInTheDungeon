using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;
        
        /// <summary>
        /// Disables body.
        /// </summary>
        /// <param name="isActive"></param>
        public virtual void SetActive(bool isActive)
        {
            if (body)
            {
                body.SetActive(isActive);
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
                gameObject.SetActive(false);
                SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                SetActive(true);
            }
        }

        public bool IsActive => body.activeSelf;
    }
}