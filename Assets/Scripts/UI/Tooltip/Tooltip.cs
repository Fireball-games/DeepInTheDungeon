using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.Tooltip
{
    /// <summary>
    /// Manages the Tooltip. Tooltip connectors should call Show() and Hide() on this class.
    /// </summary>
    public class Tooltip : Singleton<Tooltip>
    {
        private static TMP_Text _title;
        private static Image _titleBackground;
        private static TMP_Text _description;
        private static Image _descriptionBackground;

        private static RectTransform _tooltipRect;
        private static Transform _tooltipTransform;
        private static GameObject _tooltipGameObject;
        private static TooltipSettings _defaultSettings;

        protected override void Awake()
        {
            base.Awake();
            
            _tooltipRect = transform.Find("Body").GetComponent<RectTransform>();
            _tooltipTransform = _tooltipRect.transform;
            _tooltipGameObject = _tooltipRect.gameObject;
            
            _titleBackground = transform.Find("Body/Title").GetComponent<Image>();
            _title = _titleBackground.transform.Find("TitleText").GetComponent<TMP_Text>();
            _descriptionBackground = _titleBackground.transform.Find("Description").GetComponent<Image>();
            _description = _descriptionBackground.transform.Find("DescriptionText").GetComponent<TMP_Text>();

            _defaultSettings = new TooltipSettings();
            Hide();
        }

        public static void Show(RectTransform targetTransform, IEnumerable<string> strings, TooltipSettings settings = null)
        {
            if (strings == null || !strings.Any())
            {
                Logger.LogWarning("Strings are null or empty.");
                return;
            }
            
            // _tooltipRect.SetAsLastSibling();

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
            
            _descriptionBackground.gameObject.SetActive(strings.Count() > 1);
            _tooltipGameObject.SetActive(true);
        }

        public static void Hide()
        {
            _title.text = "";
            _description.text = "";
            _tooltipGameObject.SetActive(false);
        }
        
        private static void SetTooltipPosition(RectTransform targetTransform)
        {
            Vector3 resultPosition = targetTransform.position - new Vector3(targetTransform.rect.width / 2,
                targetTransform.rect.height / 2,
                -_tooltipRect.position.z);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CameraManager.Instance.mainCamera, resultPosition);

            if (screenPoint.x - (_tooltipRect.rect.width / 2) < 0)
            {
                resultPosition.x += Mathf.Abs(screenPoint.x - (_tooltipRect.rect.width / 2));
            }
            else if (screenPoint.x + (_tooltipRect.rect.width / 2) > Screen.width)
            {
                resultPosition.x -= Mathf.Abs(screenPoint.x + (_tooltipRect.rect.width / 2) - Screen.width);
            }

            if (screenPoint.y - (_tooltipRect.rect.height / 2) < 0)
            {
                resultPosition.y += Mathf.Abs(screenPoint.y - (_tooltipRect.rect.height / 2));
            }
            else if (screenPoint.y + (_tooltipRect.rect.height / 2) > Screen.height)
            {
                resultPosition.y -= Mathf.Abs(screenPoint.y + (_tooltipRect.rect.height / 2) - Screen.height);
            }

            _tooltipTransform.position = resultPosition;
        }
    }
    
    [Serializable]
    public class TooltipSettings
    {
        public Color titleTextColor = Colors.Yellow;
        public Color titleBackgroundColor = Colors.DeepBlue;
        public Color descriptionTextColor = Colors.Beige;
        public Color descriptionBackgroundColor = Colors.LightBlue;
    }
}