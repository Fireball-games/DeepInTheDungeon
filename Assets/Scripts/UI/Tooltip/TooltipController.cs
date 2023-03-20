using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.Tooltip
{
    /// <summary>
    /// Manages the Tooltip. Tooltip connectors should call Show() and Hide() on this class.
    /// </summary>
    public class TooltipController : MonoBehaviour
    {
        private TMP_Text _title;
        private Image _titleBackground;
        private TMP_Text _description;
        private Image _descriptionBackground;

        private Transform _homeTransform;
        private RectTransform _tooltipRect;
        private RectTransform _titleRect;
        private Transform _tooltipTransform;
        private GameObject _tooltipGameObject;
        private TooltipSettings _defaultSettings;

        private void Awake()
        {
            AssignReferences();

            _defaultSettings = new TooltipSettings();
            Hide();
        }

        public void Show(RectTransform targetTransform, IEnumerable<string> strings, TooltipSettings settings = null)
        {
            if (strings == null || !strings.Any())
            {
                Logger.LogWarning("Strings are null or empty.");
                return;
            }

            settings ??= _defaultSettings;
            
            _title.text = strings.ElementAt(0);
            _description.text = strings.Skip(1).ToLines();
            _title.color = settings.titleTextColor;
            _titleBackground.color = settings.titleBackgroundColor;
            
            if (strings.Count() > 1)
            {
                _description.color = settings.descriptionTextColor;
                _descriptionBackground.color = settings.descriptionBackgroundColor;
            } 
            
            SetTooltipPosition(targetTransform);
            _tooltipTransform.localScale = Vector3.one;
            
            _descriptionBackground.gameObject.SetActive(strings.Count() > 1);
            _tooltipGameObject.SetActive(true);
        }

        public void Hide()
        {
            _title.text = "";
            _description.text = "";
            _tooltipTransform.SetParent(_homeTransform);
            _tooltipGameObject.SetActive(false);
        }
        
        private void SetTooltipPosition(RectTransform targetTransform)
        {
            _tooltipTransform.SetParent(targetTransform);
            _tooltipTransform.SetAsLastSibling();
            
            Vector3 newPos = new(0, -targetTransform.rect.height, -1);
            
            Vector2 screenPoint = CameraManager.Instance.mainCamera.WorldToScreenPoint(newPos);
            Logger.Log($"Screen point: {screenPoint}");
            Vector2 offset = Vector2.zero;

            if (screenPoint.x < 0)
            {
                offset.x -= screenPoint.x;
            }
            else if (screenPoint.x + _titleRect.rect.width > Screen.width)
            {
                offset.x -= (screenPoint.x + _titleRect.rect.width - Screen.width);
            }

            if (screenPoint.y < 0)
            {
                offset.y -= screenPoint.y;
            }
            else if (screenPoint.y + _titleRect.rect.height > Screen.height)
            {
                offset.y -= (screenPoint.y + _titleRect.rect.height - Screen.height);
            }

// Check if tooltip is outside the top of the screen
            if (screenPoint.y + offset.y < 0)
            {
                offset.y = -screenPoint.y;
            }

// Check if tooltip is outside the left of the screen
            if (screenPoint.x + offset.x < 0)
            {
                offset.x = -screenPoint.x;
            }

            newPos += new Vector3(offset.x, offset.y, 0);

            _tooltipRect.anchoredPosition = newPos;
        }

        private void AssignReferences()
        {
            _homeTransform = transform.parent;
            _tooltipTransform = transform;
            _tooltipGameObject = _tooltipTransform.gameObject;
            
            _titleBackground = _tooltipTransform.Find("Body/Title").GetComponent<Image>();
            _tooltipRect = _tooltipGameObject.GetComponent<RectTransform>();
            _title = _titleBackground.transform.Find("TitleText").GetComponent<TMP_Text>();
            _titleRect = _title.GetComponent<RectTransform>();
            _descriptionBackground = _titleBackground.transform.Find("Description").GetComponent<Image>();
            _description = _descriptionBackground.transform.Find("DescriptionText").GetComponent<TMP_Text>();
        }
    }
    
    [Serializable]
    public class TooltipSettings
    {
        public Color titleTextColor = Colors.Black;
        public Color titleBackgroundColor = Colors.Beige;
        public Color descriptionTextColor = Colors.White;
        public Color descriptionBackgroundColor = Colors.DeepBlue;
    }
}