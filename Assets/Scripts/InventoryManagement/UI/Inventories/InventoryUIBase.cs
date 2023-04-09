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

        public virtual void ToggleOpen() => SetActive(!body.activeSelf);
        
        public void Close() => SetActive(false);

        public abstract void OnInitialize();
        
        protected abstract void SetTitle();
    }
}