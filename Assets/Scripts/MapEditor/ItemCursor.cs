using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Unity.Mathematics;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class ItemCursor : MonoBase
    {
        private GameObject _item;
        public ItemCursor Instance { get; private set; }

        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else Instance = this;
            
            SetActive(false);
        }

        public void Show(GameObject item = null)
        {
            if (_item) _item.Dismiss();
            
            if (item) _item = ObjectPool.Instance.Get(item, Vector3.zero, quaternion.identity, body);
            
            SetActive(true);
        }
        
        public void Hide()
        {
            if (_item) _item.Dismiss();
            SetActive(false);
        }
    }
}