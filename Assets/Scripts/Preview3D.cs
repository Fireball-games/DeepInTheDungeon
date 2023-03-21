using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts
{
    public class Preview3D : MonoBehaviour
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
        
        public enum EPreviewType
        {
            Item,
            Tile
        }

        private void Awake()
        {
            
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
            // TODO: For debug only
            // ShowPreview(null, EPreviewType.Item);
        }

        public RenderTexture ShowPreview(GameObject prefab, EPreviewType previewType)
        {
            _previewAnchor.DismissAllChildrenToPool();
            
            ObjectPool.Instance.Get(prefab, _previewAnchor);

            _camera.transform.position = previewType switch
            {
                EPreviewType.Item => _itemStartPosition + _itemStartPosition.normalized,
                EPreviewType.Tile => _tileStartPosition + _tileStartPosition.normalized,
                _ => throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null)
            };
            
            _body.SetActive(true);
            isActive = true;
            
            _camera.transform.DOMove(previewType switch
            {
                EPreviewType.Item => _itemStartPosition,
                EPreviewType.Tile => _tileStartPosition,
                _ => throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null)
            }, 1f).SetEase(Ease.OutCubic).SetAutoKill(true).Play();
            
            StartCoroutine(RenderPreviewCoroutine());
            return _previewTexture;
        }
        
        public void HidePreview()
        {
            isActive = false;
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