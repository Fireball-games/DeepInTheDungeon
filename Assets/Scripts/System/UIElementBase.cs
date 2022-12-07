using UnityEngine;

namespace Scripts.System
{
    public class UIElementBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;

        protected void SetActive(bool isActive)
        {
            if (body)
            {
                body.SetActive(isActive);
            }
        }
    }
}