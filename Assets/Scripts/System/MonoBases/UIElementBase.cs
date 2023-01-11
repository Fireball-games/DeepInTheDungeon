using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;

        public virtual void SetActive(bool isActive)
        {
            if (body)
            {
                body.SetActive(isActive);
            }
        }

        public bool IsActive => body.activeSelf;
    }
}