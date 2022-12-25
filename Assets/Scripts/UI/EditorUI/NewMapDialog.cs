using System;
using Scripts.Localization;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public sealed class NewMapDialog : DialogBase
    {
        [SerializeField] private int rowsCount;
        [SerializeField] private int columnsCount;
        [SerializeField] private int floorsCount;
        public InputField rowsInput;
        public InputField columnsInput;
        public InputField floorsInput;
        public InputField mapNameInput;

        private void OnEnable()
        {
            rowsInput.SetInputText(rowsCount.ToString());
            rowsInput.SetTitleText(t.Get(Keys.Rows));
            columnsInput.SetInputText(columnsCount.ToString());
            columnsInput.SetTitleText(t.Get(Keys.Columns));
            floorsInput.SetInputText(floorsCount.ToString());
            floorsInput.SetTitleText(t.Get(Keys.Floors));
            mapNameInput.SetPlaceholderText(t.Get(Keys.NewMapNamePrompt));
            mapNameInput.SetTitleText(t.Get(Keys.NewMapName));
        }

        public void Open(
            string dialogTitle,
            string placeholderMapName,
            Action onOk = null,
            Action onCancel = null)
        {
            mapNameInput.SetInputText("");
            base.Open(dialogTitle, onOk, onCancel, t.Get(Keys.CreateMap));
            
            mapNameInput.SetPlaceholderText(placeholderMapName);
        }
    }
}
