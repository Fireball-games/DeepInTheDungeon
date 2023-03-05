using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private List<MainMenuBase> _mainMenus;
        private MainMenuBase _mainMenuOnUI;
        private GraphicRaycaster _graphicRaycaster;
        
        [Flags]
        public enum ETargetedMainMenu
        {
            OnUi = 1 << 0,
            OnWorldCanvas = 1 << 1,
            Both = OnUi | OnWorldCanvas,
        } 

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
        
        public async Task ShowMainMenu(bool show, ETargetedMainMenu targetedMainMenu = ETargetedMainMenu.Both)
        {
            List<Task> tasks = new();
            
            foreach (MainMenuBase menu in _mainMenus)
            {
                switch (menu)
                {
                    case MainMenuOnUI when targetedMainMenu.HasFlag(ETargetedMainMenu.OnUi):
                    case MainMenuWorld when targetedMainMenu.HasFlag(ETargetedMainMenu.OnWorldCanvas):
                        tasks.Add(menu.SetActiveAsync(show));
                        break;
                }
            }
            
            await Task.WhenAll(tasks);
        }

        public async void ShowCrossHairAsync(bool show) => await _crossHair.SetActiveAsync(show);
        
        public void RefreshMainMenuButtons() => _mainMenus.ForEach(menu => menu.RefreshMainMenuButtons());

        private void OnLevelStarted()
        {
            MainMenuCarrier mainMenuCarrier = FindObjectOfType<MainMenuCarrier>();
            
            if (!mainMenuCarrier) return;
            
            mainMenuCarrier.mainMenuCanvas.worldCamera = CameraManager.Instance.mainCamera;
            // mainMenuCarrier.mainMenu.SetActive(true);
            _mainMenus.Add(mainMenuCarrier.mainMenu);
        }
        
        private void AssignComponents()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            
            _crossHair = body.Find("CrossHair").GetComponent<ImageUIElement>();
            // _mainMenuOnUI = body.Find("MainEscapeMenu/MainMenu").GetComponent<MainMenuBase>();

            _mainMenus = new List<MainMenuBase>();
            // {
            //     _mainMenuOnUI,
            // };

            GetComponentInChildren<MainEscapeMenu>(true).SetGraphicRaycaster(_graphicRaycaster);
        }

        public void GraphicRaycasterEnabled(bool isEnabled)
        {
            if (!_graphicRaycaster) return;
            
            _graphicRaycaster.enabled = isEnabled;
        }

        public void OpenLoadMenu() => _mainMenus.ForEach(menu => menu.OpenLoadMenu());
    }
}
