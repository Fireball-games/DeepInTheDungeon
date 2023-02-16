using System.Collections.Generic;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using Scripts.UI.PlayMode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class MainUIManager : SingletonNotPersisting<MainUIManager>
    {
        [SerializeField] private Transform body;
        private ImageUIElement _crossHair;

        private List<MainMenu> _mainMenus;
        private MainMenu _mainMenuOnUI;
        private GraphicRaycaster _graphicRaycaster;

        protected override void Awake()
        {
            base.Awake();
            
            AssignComponents();
        }

        private void OnEnable()
        {
            EventsManager.OnLevelStarted += OnLevelStarted;
        }

        private void OnDisable()
        {
            EventsManager.OnLevelStarted -= OnLevelStarted;
        }
        
        public void ShowMainMenu(bool show)
        {
            _mainMenus.ForEach(menu => menu.SetActive(show));
        }

        public void ShowCrossHair(bool show) => _crossHair.SetActive(show);
        
        public void RefreshMainMenuButtons() => _mainMenus.ForEach(menu => menu.RefreshButtons());

        private void OnLevelStarted()
        {
            MainMenuCarrier mainMenuCarrier = FindObjectOfType<MainMenuCarrier>();

            if (!mainMenuCarrier) return;
            
            mainMenuCarrier.mainMenuCanvas.worldCamera = CameraManager.Instance.mainCamera;
            mainMenuCarrier.mainMenu.SetActive(true);
            _mainMenus.Add(mainMenuCarrier.mainMenu);
        }
        
        private void AssignComponents()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            
            _crossHair = body.Find("CrossHair").GetComponent<ImageUIElement>();
            _mainMenuOnUI = body.Find("MainEscapeMenu/MainMenu").GetComponent<MainMenu>();
            
            _mainMenus = new List<MainMenu>
            {
                _mainMenuOnUI,
            };

            GetComponentInChildren<MainEscapeMenu>().SetGraphicRaycaster(_graphicRaycaster);
        }

        public void GraphicRaycasterEnabled(bool isEnabled)
        {
            if (!_graphicRaycaster) return;
            
            _graphicRaycaster.enabled = isEnabled;
        }
    }
}
