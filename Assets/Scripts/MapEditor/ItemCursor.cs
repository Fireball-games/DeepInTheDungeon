using System.Collections;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor.Services;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Unity.Mathematics;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class ItemCursor : SingletonNotPersisting<ItemCursor>
    {
        [SerializeField] private Vector3 offset = new(0, -0.8f, 0);
        private GameObject _highlight;
        private GameObject _body;
        private GameObject _item;
        private bool _isActive;

        protected override void Awake()
        {
            base.Awake();
            
            _body = transform.Find("Body").gameObject;
            _highlight = _body.transform.Find("Highlight").gameObject;
            SetBodyActive(false);
        }

        public void Show(GameObject item = null)
        {
            if (item) ShowItem(item);
            
            _isActive = true;
            StartCoroutine(PositionByMouseCoroutine());
            SetBodyActive(true);
        }

        private IEnumerator PositionByMouseCoroutine()
        {
            while (_isActive)
            {
                SetBodyActive(!EditorMouseService.Instance.IsOverUI && EditorMouseService.Instance.GridPositionType is Enums.EGridPositionType.EditableTile);
                transform.position = EditorMouseService.Instance.MousePositionOnPlane + offset;
                yield return null;
            }
        }

        public void Hide()
        {
            _isActive = false;
            if (_item) _item.DismissToPool();
            _highlight.SetActive(false);
            SetBodyActive(false);
        }
        
        public void AddItem(GameObject item) => ShowItem(item);

        private void ShowItem(GameObject item)
        {
            if (_item) _item.DismissToPool();
            _highlight.SetActive(true);
            _item = ObjectPool.Instance.Get(item, _body);
            _item.transform.localPosition = V3Extensions.Zero;
            _item.transform.localRotation = quaternion.identity;
        }

        private void SetBodyActive(bool value) => _body.SetActive(value);
    }
}