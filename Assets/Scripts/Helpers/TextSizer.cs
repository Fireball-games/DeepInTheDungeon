// ***************************************************************************************************************
// ******** Credits due: https://forum.unity.com/threads/does-the-content-size-fitter-work.484678/ Akrone ********
// ***************************************************************************************************************

using System;
using TMPro;
using UnityEngine;

namespace Scripts.Helpers
{
    [ExecuteInEditMode]
    public class TextSizer : MonoBehaviour
    {
        public TMP_Text text;
        public bool resizeTextObject = true;
        public Vector2 padding;
        public Vector2 maxSize = new(1000, float.PositiveInfinity);
        public Vector2 minSize;
        public Mode controlAxes = Mode.Both;

        [Flags]
        public enum Mode
        {
            None = 0,
            Horizontal = 0x1,
            Vertical = 0x2,
            Both = Horizontal | Vertical
        }

        private string _lastText;
        private Mode _lastControlAxes = Mode.None;
        private Vector2 _lastSize;
        private bool _forceRefresh;
        private bool _isTextNull = true;
        private RectTransform _textRectTransform;
        private RectTransform _selfRectTransform;

        protected virtual float MinX
        {
            get
            {
                if ((controlAxes & Mode.Horizontal) != 0) return minSize.x;
                return _selfRectTransform.rect.width - padding.x;
            }
        }

        protected virtual float MinY
        {
            get
            {
                if ((controlAxes & Mode.Vertical) != 0) return minSize.y;
                return _selfRectTransform.rect.height - padding.y;
            }
        }

        protected virtual float MaxX
        {
            get
            {
                if ((controlAxes & Mode.Horizontal) != 0) return maxSize.x;
                return _selfRectTransform.rect.width - padding.x;
            }
        }

        protected virtual float MaxY
        {
            get
            {
                if ((controlAxes & Mode.Vertical) != 0) return maxSize.y;
                return _selfRectTransform.rect.height - padding.y;
            }
        }

        protected virtual void Update()
        {
            if (!_isTextNull && (text.text != _lastText || _lastSize != _selfRectTransform.rect.size || _forceRefresh ||
                                 controlAxes != _lastControlAxes))
            {
                var preferredSize = text.GetPreferredValues(MaxX, MaxY);
                preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
                preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
                preferredSize += padding;

                if ((controlAxes & Mode.Horizontal) != 0)
                {
                    _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    if (resizeTextObject)
                    {
                        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    }
                }

                if ((controlAxes & Mode.Vertical) != 0)
                {
                    _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    if (resizeTextObject)
                    {
                        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    }
                }

                _lastText = text.text;
                _lastSize = _selfRectTransform.rect.size;
                _lastControlAxes = controlAxes;
                _forceRefresh = false;
            }
        }

        // Forces a size recalculation on next Update
        public virtual void Refresh()
        {
            _forceRefresh = true;

            _isTextNull = text == null;
            if (text) _textRectTransform = text.GetComponent<RectTransform>();
            _selfRectTransform = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            Refresh();
        }
    }
}