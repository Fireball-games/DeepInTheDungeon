using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.Tooltip
{
    /// <summary>
    /// Manages the Tooltip. Tooltip connectors should call Show() and Hide() on this class.
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        private TMP_Text _title;
        private Image _titleBackground;
        private TMP_Text _description;
        private Image _descriptionBackground;

        private RectTransform _tooltipRect;
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

            if (!_tooltipTransform) AssignReferences();

            _tooltipTransform.SetParent(targetTransform);
            _tooltipTransform.SetAsLastSibling();

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
            _tooltipTransform.SetParent(null);
            _tooltipGameObject.SetActive(false);
        }
        
        private void SetTooltipPosition(RectTransform targetTransform)
        {
            Vector3 resultPosition = targetTransform.position - new Vector3(targetTransform.rect.width / 2,
                targetTransform.rect.height / 2,
                -_tooltipRect.position.z);
            // TODO: Fix this, currently, it's not working properly.
            // Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CameraManager.Instance.mainCamera, resultPosition);
            //
            // if (screenPoint.x - (_tooltipRect.rect.width / 2) < 0)
            // {
            //     resultPosition.x += Mathf.Abs(screenPoint.x - (_tooltipRect.rect.width / 2));
            // }
            // else if (screenPoint.x + (_tooltipRect.rect.width / 2) > Screen.width)
            // {
            //     resultPosition.x -= Mathf.Abs(screenPoint.x + (_tooltipRect.rect.width / 2) - Screen.width);
            // }
            //
            // if (screenPoint.y - (_tooltipRect.rect.height / 2) < 0)
            // {
            //     resultPosition.y += Mathf.Abs(screenPoint.y - (_tooltipRect.rect.height / 2));
            // }
            // else if (screenPoint.y + (_tooltipRect.rect.height / 2) > Screen.height)
            // {
            //     resultPosition.y -= Mathf.Abs(screenPoint.y + (_tooltipRect.rect.height / 2) - Screen.height);
            // }

            _tooltipTransform.position = resultPosition;
        }

        private void AssignReferences()
        {
            _tooltipTransform = transform;
            _tooltipGameObject = _tooltipTransform.gameObject;
            
            _titleBackground = _tooltipTransform.Find("Body/Title").GetComponent<Image>();
            _tooltipRect = _titleBackground.GetComponent<RectTransform>();
            _title = _titleBackground.transform.Find("TitleText").GetComponent<TMP_Text>();
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