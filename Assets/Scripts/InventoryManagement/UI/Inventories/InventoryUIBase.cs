using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;

namespace Scripts.InventoryManagement.UI.Inventories
{
    public abstract class InventoryUIBase : UIElementBase
    {
        [SerializeField] protected TMP_Text title;

        protected virtual void Awake()
        {
            SetTitle();
        }

        public void ToggleOpen() => SetActive(!body.activeSelf);
        
        public void Close() => SetActive(false);
        
        protected abstract void SetTitle();
    }
}