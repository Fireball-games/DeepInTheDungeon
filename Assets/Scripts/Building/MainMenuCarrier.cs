using System;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Building
{
    public class MainMenuCarrier : MonoBehaviour
    {
        public Canvas mainMenuCanvas;
        public MainMenu mainMenu;

        private void Awake()
        {
            mainMenuCanvas ??= GetComponentInChildren<Canvas>();
            mainMenu ??= GetComponentInChildren<MainMenu>();
        }
    }
}