using System;
using System.Collections;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class ImageButton : UIElementBase
    {
        [Header("Sprites")] [SerializeField] private Sprite frame;
        [SerializeField] protected Sprite icon;
        [SerializeField] private Sprite background;
        [Header("Colors")] [SerializeField] private Color idleColor;
        [SerializeField] private Color enteredColor;
        [SerializeField] private Color clickedColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color selectedEnteredColor;
        [Header("Options")] [SerializeField] private float clickedEffectDuration = 0.2f;
        [SerializeField] private MouseClickOverlay mouseClickOverlay;

        [Header("Assignables")] 
        [SerializeField] private Image frameImage;
        [SerializeField] protected Image iconImage;
        [SerializeField] private Image backgroundImage;

        public event Action<ImageButton> OnClickWithSender;
        public virtual event Action OnClick;
        public event Action OnSelected;
        public event Action OnDeselected;

        private bool _isMouseEntered;
        private bool _isSelected;

        private bool _isInteractable = true;

        private bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                _isInteractable = value;

                if (value)
                {
                    SetBackgroundColor();
                }
                else
                {
                    backgroundImage.color = enteredColor;
                }
            }
        }

        private void OnEnable()
        {
            mouseClickOverlay.OnClick += OnClickInternal;
            mouseClickOverlay.OnMouseEnter += OnMouseEnter;
            mouseClickOverlay.OnMouseLeave += OnMouseExit;

            SetBackgroundColor();
        }

        protected virtual void OnDisable()
        {
            _isMouseEntered = false;
            
            backgroundImage.color = idleColor;
            StopAllCoroutines();

            OnClick = null;
            mouseClickOverlay.OnClick -= OnClickInternal;
            mouseClickOverlay.OnMouseEnter -= OnMouseEnter;
            mouseClickOverlay.OnMouseLeave -= OnMouseExit;
        }

        public void SetInteractable(bool isInteractable) => IsInteractable = isInteractable;

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

        protected virtual void OnClickInternal()
        {
            if (!IsInteractable) return;
            
            OnClickWithSender?.Invoke(this);
            OnClick?.Invoke();
            
            if (isActiveAndEnabled)
            {
                StartCoroutine(ClickedCoroutine());
            }
        }

        private void OnMouseEnter()
        {
            _isMouseEntered = true;
            
            if (!IsInteractable) return;
            
            SetBackgroundColor();
        }

        private void OnMouseExit()
        {
            _isMouseEntered = false;

            if (!IsInteractable) return;
            
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
        protected virtual void OnValidate()
        {
            if (frame != null)
            {
                frameImage.sprite = frame;
            }

            if (background != null)
            {
                backgroundImage.sprite = background;
            }

            if (iconImage != null)
            {
                iconImage.sprite = icon;
            }
            
            backgroundImage.color = idleColor;
        }
#endif
    }
}