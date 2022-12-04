using UnityEngine;

namespace Scripts.System
{
    public class UIWindowBase : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject content;

        public void SetActive(bool isActive) => body.SetActive(isActive);
    }
}