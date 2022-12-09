using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class ImageButton : MonoBehaviour
    {
        [Header("Sprites")] [SerializeField] private Sprite frame;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite background;
        [Header("Colors")] [SerializeField] private Color idleColor;
        [SerializeField] private Color enteredColor;
        [SerializeField] private Color clickedColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color selectedEnteredColor;
        [Header("Options")] [SerializeField] private float clickedEffectDuration = 0.2f;
        [SerializeField] private MouseClickOverlay mouseClickOverlay;

        [Header("Assignables")] [SerializeField]
        private Image frameImage;

        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;

        public event Action<ImageButton> OnClickWithSender;
        public event Action OnClick;
        public event Action OnSelected;
        public event Action OnDeselected;

        private bool _isMouseEntered;
        private bool _isSelected;

        private void OnEnable()
        {
            mouseClickOverlay.OnClick += OnClickInternal;
            mouseClickOverlay.OnMouseEnter += OnMouseEnter;
            mouseClickOverlay.OnMouseLeave += OnMouseExit;

            SetBackgroundColor();
        }

        public void SetActive(bool isActive) => gameObject.SetActive(isActive); 

        public void SetSelected(bool isSelected, bool silent = false)
        {
            _isSelected = isSelected;
            SetBackgroundColor();

            if (silent) return;
            
            if (isSelected)
                OnSelected?.Invoke();
            else
                OnDeselected?.Invoke();
        }

        private void OnClickInternal()
        {
            OnClickWithSender?.Invoke(this);
            OnClick?.Invoke();
            StartCoroutine(ClickedCoroutine());
        }

        private void OnMouseEnter()
        {
            _isMouseEntered = true;
            SetBackgroundColor();
        }

        private void OnMouseExit()
        {
            _isMouseEntered = false;
            SetBackgroundColor();
        }

        private IEnumerator ClickedCoroutine()
        {
            backgroundImage.color = clickedColor;

            yield return new WaitForSeconds(clickedEffectDuration);

            SetBackgroundColor();
        }

        private void SetBackgroundColor()
        {
            Color result;

            if (_isMouseEntered)
            {
                result = _isSelected ? selectedEnteredColor : enteredColor;
            }
            else if (_isSelected)
            {
                result = _isMouseEntered ? selectedEnteredColor : selectedColor;
            }
            else
            {
                result = idleColor;
            }

            backgroundImage.color = result;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (frame)
            {
                frameImage.sprite = frame;
            }

            if (background)
            {
                backgroundImage.sprite = background;
            }

            if (icon)
            {
                iconImage.sprite = icon;
            }

            backgroundImage.color = idleColor;
        }
#endif
    }
}