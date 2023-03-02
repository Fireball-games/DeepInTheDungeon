using System;
using System.Collections;
using Scripts.Helpers;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.Components.Buttons
{
    public class ImageButton : UIElementBase, IPoolInitializable
    {
        [Header("Sprites")] [SerializeField] private Sprite frame;
        [SerializeField] protected Sprite icon;
        [SerializeField] private Sprite background;
        [Header("Options")] [SerializeField] private float clickedEffectDuration = 0.2f;

        [Header("Assignables")] 
        [SerializeField] private Image frameImage;
        [SerializeField] protected Image iconImage;
        [SerializeField] private Image backgroundImage;

        private Color IdleColor => Colors.ButtonIdle;
        private Color EnteredColor => Colors.ButtonEntered;
        private Color ClickedColor => Colors.ButtonClicked;
        private Color SelectedColor => Colors.Selected;
        private Color SelectedEnteredColor => Colors.SelectedOver;
        private Color DisabledColor => Colors.Disabled;
        private MouseClickOverlay _mouseClickOverlay;
        
        public event Action<ImageButton> OnClickWithSender;
        public UnityEvent OnClick { get; } = new();
        public UnityEvent OnMouseEnter { get; } = new();
        public UnityEvent OnMouseExit { get; } = new();
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
                    backgroundImage.color = DisabledColor;
                }
            }
        }

        private void Awake()
        {
            _mouseClickOverlay = GetComponentInChildren<MouseClickOverlay>();
        }

        private void OnEnable()
        {
            _mouseClickOverlay.OnClick += OnClick_Internal;
            _mouseClickOverlay.OnMouseEnter += OnMouseEnter_internal;
            _mouseClickOverlay.OnMouseLeave += OnMouseExit_internal;

            SetBackgroundColor();
        }

        protected virtual void OnDisable()
        {
            _isMouseEntered = false;
            
            backgroundImage.color = IdleColor;
            StopAllCoroutines();
            
            _mouseClickOverlay.OnClick -= OnClick_Internal;
            _mouseClickOverlay.OnMouseEnter -= OnMouseEnter_internal;
            _mouseClickOverlay.OnMouseLeave -= OnMouseExit_internal;
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

        protected virtual void OnClick_Internal()
        {
            if (!IsInteractable) return;
            
            OnClickWithSender?.Invoke(this);
            OnClick?.Invoke();
            
            if (isActiveAndEnabled)
            {
                StartCoroutine(ClickedCoroutine());
            }
        }

        private void OnMouseEnter_internal()
        {
            _isMouseEntered = true;
            
            if (!IsInteractable) return;
            
            SetBackgroundColor();
            
            OnMouseEnter.Invoke();
        }

        private void OnMouseExit_internal()
        {
            _isMouseEntered = false;

            if (!IsInteractable) return;
            
            SetBackgroundColor();
            
            OnMouseExit.Invoke();
        }

        private IEnumerator ClickedCoroutine()
        {
            backgroundImage.color = ClickedColor;

            yield return new WaitForSeconds(clickedEffectDuration);

            SetBackgroundColor();
        }

        private void SetBackgroundColor()
        {
            Color result;

            if (_isMouseEntered)
            {
                result = _isSelected ? SelectedEnteredColor : EnteredColor;
            }
            else if (_isSelected)
            {
                result = _isMouseEntered ? SelectedEnteredColor : SelectedColor;
            }
            else
            {
                result = IdleColor;
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
            
            backgroundImage.color = IdleColor;
        }
#endif
        public void InitializeFromPool()
        {
            OnClick.RemoveAllListeners();
        }
    }
}