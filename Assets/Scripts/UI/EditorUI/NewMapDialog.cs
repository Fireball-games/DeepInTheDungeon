using System;
using Scripts.Localization;
using Scripts.System.MonoBases;
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
            rowsInput.SetTitleText(T.Get(Keys.Rows));
            columnsInput.SetInputText(columnsCount.ToString());
            columnsInput.SetTitleText(T.Get(Keys.Columns));
            floorsInput.SetInputText(floorsCount.ToString());
            floorsInput.SetTitleText(T.Get(Keys.Floors));
            mapNameInput.SetPlaceholderText(T.Get(Keys.NewMapNamePrompt));
            mapNameInput.SetTitleText(T.Get(Keys.NewMapName));
        }

        public void Open(
            string dialogTitle,
            string placeholderMapName,
            Action onOk = null,
            Action onCancel = null)
        {
            base.Open(dialogTitle, onOk, onCancel, T.Get(Keys.CreateMap));
            
            mapNameInput.SetPlaceholderText(placeholderMapName);
        }
    }
}
