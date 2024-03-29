﻿using Scripts.UI.MainMenus;
using UnityEngine;

namespace Scripts.Building
{
    public class MainMenuCarrier : MonoBehaviour
    {
        public Canvas mainMenuCanvas;
        public MainMenuBase mainMenu;

        private void Awake()
        {
            mainMenuCanvas ??= GetComponentInChildren<Canvas>();
            mainMenu ??= GetComponentInChildren<MainMenuBase>();
        }
    }
}