using System.Collections.Generic;
using Scripts.EventsManagement;
using Scripts.ScenesManagement;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.PlayMode
{
    public class HUDController : UIElementBase
    {
        [SerializeField] private List<UIElementBase> hideOnStart;

        private ImageUIElement _crossHair;

        private void Awake()
        {
            _crossHair = body.transform.Find("CrossHair").GetComponent<ImageUIElement>();
        }

        private void OnEnable()
        {
            PlayEvents.OnLookModeActiveChanged += OnLookModeActiveChanged;
            EventsManager.OnLevelStarted += OnLevelStarted;
            PrepareForTransition();
        }

        private void OnDisable()
        {
            PlayEvents.OnLookModeActiveChanged -= OnLookModeActiveChanged;
            EventsManager.OnLevelStarted -= OnLevelStarted;
        }
        
        private void OnLevelStarted()
        {
            SetActive(true);
        }

        private void OnLookModeActiveChanged(bool isActive)
        {
            _crossHair.SetActive(isActive);
        }

        private void Start()
        {
            if (!SceneLoader.IsInMainScene)
            {
                hideOnStart.ForEach(e => e.SetActive(false));
            }
        }
    }
}
