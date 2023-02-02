using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            _tooltipRect = GetComponent<RectTransform>();
            _tooltipTransform = transform;
            _tooltipGameObject = gameObject;
            
            _titleBackground = transform.Find("Body/Title").GetComponent<Image>();
            _title = _titleBackground.transform.Find("TitleText").GetComponent<TMP_Text>();
            _descriptionBackground = _title.transform.Find("Description").GetComponent<Image>();
            _description = _descriptionBackground.transform.Find("DescriptionText").GetComponent<TMP_Text>();

            _defaultSettings = new TooltipSettings();
            Hide();
        }

        // public static void Show(RectTransform targetTransform, string tooltipTitle, string tooltipDescription)
        // {
        //     if (targetTransform == null) return;
        //
        //     _title.text = tooltipTitle;
        //     _description.text = tooltipDescription;
        //     _tooltipTransform.position = targetTransform.position - new Vector3
        //     {
        //         x = targetTransform.rect.width / 2,
        //         y = targetTransform.rect.height / 2
        //     };
        //
        //     _tooltipGameObject.SetActive(true);
        // }

        public void Show(RectTransform targetTransform, IEnumerable<string> strings, TooltipSettings settings = null)
        {
            if (strings == null) return;
            
            _tooltipRect.SetAsLastSibling();

            settings ??= _defaultSettings;
            
            _title.text = strings.ElementAt(0);
            _description.text = strings.Skip(1).ToLines();
            _title.color = settings.titleTextColor;
            _titleBackground.color = settings.titleBackgroundColor;
            _description.color = settings.descriptionTextColor;
            _descriptionBackground.color = settings.descriptionBackgroundColor;
            
            SetTooltipPosition(targetTransform);

            _tooltipGameObject.SetActive(true);
        }

        public void Hide()
        {
            _title.text = "";
            _description.text = "";
            gameObject.SetActive(false);
        }
        
        private void SetTooltipPosition(RectTransform targetTransform)
        {
            Vector3 resultPosition = targetTransform.position - new Vector3(targetTransform.rect.width / 2, targetTransform.rect.height / 2);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, resultPosition);

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