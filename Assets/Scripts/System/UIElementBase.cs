using UnityEngine;

namespace Scripts.System
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
    }
}