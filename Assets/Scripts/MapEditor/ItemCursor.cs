using System;
using System.Collections;
using DG.Tweening;
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
        private GameObject _body;
        private GameObject _highlight;
        private SpriteRenderer _detailImage;
        private GameObject _item;
        private bool _isActive;
        
        private Tween _detailTween;

        protected override void Awake()
        {
            base.Awake();
            
            _body = transform.Find("Body").gameObject;
            _highlight = _body.transform.Find("Highlight").gameObject;
            _detailImage = _body.transform.Find("DetailImage").GetComponent<SpriteRenderer>();
            SetBodyActive(false);
        }

        public ItemCursor Show(GameObject item = null)
        {
            if (item) ShowItem(item);
            
            _isActive = true;
            StartCoroutine(PositionByMouseCoroutine());
            SetBodyActive(true);

            return this;
        }
        
        public void SetDetailImage(Sprite sprite, Color color, Func<SpriteRenderer, Tween> detailAction = null)
        {
            _detailImage.gameObject.SetActive(true);
            _detailImage.sprite = sprite;
            _detailImage.color = color;

            if (detailAction != null)
            {
                _detailTween = detailAction.Invoke(_detailImage);
            }
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
            _detailImage.gameObject.SetActive(false);
            _detailTween?.Kill();
            SetBodyActive(false);
        }
        
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