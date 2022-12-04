using System;
using System.Collections;
using Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

public class ImageButton : MonoBehaviour
{
   [Header("Sprites")]
   [SerializeField] private Sprite frame;
   [SerializeField] private Sprite icon;
   [SerializeField] private Sprite background;
   [Header("Colors")]
   [SerializeField] private Color idleColor;
   [SerializeField] private Color enteredColor;
   [SerializeField] private Color clickedColor;
   [SerializeField] private Color selectedColor;
   [SerializeField] private Color selectedEnteredColor;
   [Header("Options")]
   [SerializeField] private float clickedEffectDuration = 0.2f;
   [SerializeField] private MouseClickOverlay mouseClickOverlay;
   [Header("Assignables")] 
   [SerializeField] private Image frameImage;
   [SerializeField] private Image iconImage;
   [SerializeField] private Image backgroundImage;

   public event Action<ImageButton> OnClick;
   
   private bool _isMouseEntered;
   private bool _isSelected;
   
   private void OnEnable()
   {
      mouseClickOverlay.OnClick += OnClickInternal;
      mouseClickOverlay.OnMouseEnter += OnMouseEnter;
      mouseClickOverlay.OnMouseLeave += OnMouseExit;
      
      SetBackgroundColor();
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

   public void SetSelected(bool isSelected)
   {
      _isSelected = isSelected;
      SetBackgroundColor();
   }

   private void OnClickInternal()
   {
      OnClick?.Invoke(this);
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
}
