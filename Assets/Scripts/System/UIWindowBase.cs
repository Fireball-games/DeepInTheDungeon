using UnityEngine;

namespace Scripts.System
{
    public class UIWindowBase : MonoBehaviour
    {
        [SerializeField] protected GameObject body;
        [SerializeField] protected GameObject content;

        public void SetActive(bool isActive) => body.SetActive(isActive);
    }
}