using System.Collections;
using Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class ImageButtonController : MonoBehaviour
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
   [Header("Options")]
   [SerializeField] private float clickedEffectDuration = 0.2f;
   [SerializeField] private MouseClickOverlay mouseClickOverlay;
   [Header("Assignables")] 
   [SerializeField] private Image frameImage;
   [SerializeField] private Image iconImage;
   [SerializeField] private Image backgroundImage;

   private bool _mouseEntered;

   private void OnEnable()
   {
      mouseClickOverlay.OnClick += OnClick;
      mouseClickOverlay.OnMouseEnter += OnMouseEnter;
      mouseClickOverlay.OnMouseLeave += OnMouseExit;
      
      SetBackgroundColor();
   }

   private void OnClick()
   {
      StartCoroutine(ClickedCoroutine());
   }
   
   private void OnMouseEnter()
   {
      _mouseEntered = true;
      SetBackgroundColor();
   }
   
   private void OnMouseExit()
   {
      _mouseEntered = false;
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
      backgroundImage.color = _mouseEntered ? enteredColor : idleColor;
   }
}
