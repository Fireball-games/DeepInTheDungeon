using System;
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

        [NonSerialized] public ImageUIElement CrossHair;

        private void Awake()
        {
            CrossHair = body.transform.Find("CrossHair").GetComponent<ImageUIElement>();
        }

        private void OnEnable()
        {
            PlayEvents.OnLookModeActiveChanged += OnLookModeActiveChanged;
        }

        private void OnDisable()
        {
            PlayEvents.OnLookModeActiveChanged -= OnLookModeActiveChanged;
        }

        private void OnLookModeActiveChanged(bool isActive)
        {
            CrossHair.SetActive(isActive);
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
