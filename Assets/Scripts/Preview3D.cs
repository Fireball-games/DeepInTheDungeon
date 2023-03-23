using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts
{
    public class Preview3D : SingletonNotPersisting<Preview3D>
    {
        [SerializeField, ReadOnly] private bool isActive;
        private GameObject _body;
        private Camera _camera;
        private GameObject _previewAnchor;
        private RenderTexture _previewTexture;
        private Transform _itemPreviewPosition;
        private Transform _tilePreviewPosition;
        private Vector3 _itemStartPosition;
        private Vector3 _tileStartPosition;
        
        private Tween _zoomTween;
        
        public enum EPreviewType
        {
            Item,
            Tile
        }

        protected override void Awake()
        {
            base.Awake();
            
            _body = transform.Find("Body").gameObject;
            _camera = transform.Find("Body/PreviewCamera").GetComponent<Camera>();
            _previewAnchor = transform.Find("Body/PreviewObjectAnchor").gameObject;
            _itemPreviewPosition = transform.Find("Body/ItemPreviewPosition");
            _itemStartPosition = _itemPreviewPosition.position;
            _tilePreviewPosition = transform.Find("Body/TilePreviewPosition");
            _tileStartPosition = _tilePreviewPosition.position;
            _previewTexture = new RenderTexture(256, 256, 24);
            _camera.targetTexture = _previewTexture;
            
            _body.SetActive(false);
        }

        public RenderTexture Show(GameObject prefab, EPreviewType previewType)
        {
            _previewAnchor.DismissAllChildrenToPool();
            
            GameObject go = ObjectPool.Instance.Get(prefab, _previewAnchor);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            _camera.transform.position = previewType switch
            {
                EPreviewType.Item => _itemStartPosition + _itemStartPosition.normalized,
                EPreviewType.Tile => _tileStartPosition + _tileStartPosition.normalized,
                _ => throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null)
            };
            
            _body.SetActive(true);
            isActive = true;
            
            if (_zoomTween != null && _zoomTween.IsActive())
                _zoomTween.Kill();
            
            _zoomTween = _camera.transform.DOMove(previewType switch
            {
                EPreviewType.Item => _itemStartPosition,
                EPreviewType.Tile => _tileStartPosition,
                _ => throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null)
            }, 1f).SetEase(Ease.OutCubic).SetAutoKill(true).Play();
            
            StartCoroutine(RenderPreviewCoroutine());
            return _previewTexture;
        }
        
        public void Hide()
        {
            isActive = false;
            
            if (!_body) return;
            
            _body.SetActive(false);
        }

        private IEnumerator RenderPreviewCoroutine()
        {
            while (isActive)
            {
                _camera.Render();
                yield return null;
            }
        }
    }
}