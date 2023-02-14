using System;
using System.Collections.Generic;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI
{
    public class MainUIManager : SingletonNotPersisting<MainUIManager>
    {
        [SerializeField] private Transform body;
        private ImageUIElement _crossHair;

        private List<MainMenu> _mainMenus;

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
            MainMenuTile mainMenuTile = FindObjectOfType<MainMenuTile>();

            if (!mainMenuTile) return;
            
            mainMenuTile.mainMenuCanvas.worldCamera = CameraManager.Instance.mainCamera;
            _mainMenus.Add(mainMenuTile.mainMenu);
        }
        
        private void AssignComponents()
        {
            _crossHair = body.Find("CrossHair").GetComponent<ImageUIElement>();
            
            _mainMenus = new List<MainMenu>
            {
                body.Find("MainEscapeMenu/MainMenu").GetComponent<MainMenu>(),
            };
        }
    }
}
