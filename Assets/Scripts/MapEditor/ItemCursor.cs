﻿using System;
using System.Collections;
using DG.Tweening;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor.Services;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI.PrefabEditors.ItemEditing;
using Unity.Mathematics;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class ItemCursor : SingletonNotPersisting<ItemCursor>
    {
        [SerializeField] private Vector3 defaultOffset = new(0, -0.8f, 0);
        private GameObject _body;
        private GameObject _highlight;
        private SpriteRenderer _detailImage;
        private GameObject _item;
        private bool _isActive;
        
        private Tween _detailTween;
        private Vector3 _offset;

        protected override void Awake()
        {
            base.Awake();
            
            _body = transform.Find("Body").gameObject;
            _highlight = _body.transform.Find("Highlight").gameObject;
            _detailImage = _body.transform.Find("DetailImage").GetComponent<SpriteRenderer>();
            SetBodyActive(false);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            RemoveDetailImage();
        }

        public ItemCursor ShowAndFollowMouse(GameObject item = null)
        {
            Show(transform.position, item);

            StartCoroutine(PositionByMouseCoroutine());
            return this;
        }
        
        public ItemCursor Show(Vector3 position, GameObject item = null)
        {
            ShowItem(item);
            StopAllCoroutines();
            _isActive = true;
            SetBodyActive(true);
            transform.position = position + _offset;

            return this;
        }
        
        public ItemCursor WithOffset(Vector3 newOffset)
        {
            _offset = newOffset;
            
            return this;
        }

        public void SetDetailImage(DetailCursorSetup setup) => SetDetailImage(setup.Image, Colors.Get(setup.Color), setup.DetailTweenFunc);

        public void SetDetailImage(Sprite sprite, Color color, Func<SpriteRenderer, Tween> detailAction = null)
        {
            _detailImage.gameObject.SetActive(true);
            _detailImage.sprite = sprite;
            _detailImage.color = color;

            if (detailAction != null)
            {
                _detailTween = detailAction.Invoke(_detailImage).Play();
            }
        }
        
        public void Highlight(bool isHighlighted) => _highlight.SetActive(isHighlighted);

        public void Hide()
        {
            _isActive = false;
            if (_item) _item.DismissToPool();
            _highlight.SetActive(false);
            _offset = defaultOffset;
            RemoveDetailImage();
            SetBodyActive(false);
        }
        
        private IEnumerator PositionByMouseCoroutine()
        {
            while (_isActive)
            {
                SetBodyActive(!EditorMouseService.Instance.IsOverUI && EditorMouseService.Instance.GridPositionType is Enums.EGridPositionType.EditableTile);
                transform.position = EditorMouseService.Instance.MousePositionOnPlane + _offset;
                yield return null;
            }
        }

        private void ShowItem(GameObject item)
        {
            if (_item)
            {
                _item.DismissToPool();
                _item = null;
            }
            
            if (!item) return;
            
            _highlight.SetActive(true);
            _item = ObjectPool.Instance.Get(item, _body);
            _item.transform.localPosition = V3Extensions.Zero;
            _item.transform.localRotation = quaternion.identity;
        }
        
        private void RemoveDetailImage()
        {
            _detailImage.gameObject.SetActive(false);
            _detailTween?.Kill();
        }

        private void SetBodyActive(bool value) => _body.SetActive(value);
    }
}