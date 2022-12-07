using System;
using Scripts.Localization;
using Scripts.System;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public sealed class NewMapDialog : DialogBase
    {
        [SerializeField] private int rowsCount;
        [SerializeField] private int columnsCount;
        public InputField rowsInput;
        public InputField columnsInput;
        public InputField mapNameInput;

        private void OnEnable()
        {
            rowsInput.SetInputText(rowsCount.ToString());
            rowsInput.SetTitleText(T.Get(LocalizationKeys.Rows));
            columnsInput.SetInputText(columnsCount.ToString());
            columnsInput.SetTitleText(T.Get(LocalizationKeys.Columns));
            mapNameInput.SetPlaceholderText(T.Get(LocalizationKeys.NewMapNamePrompt));
            mapNameInput.SetTitleText(T.Get(LocalizationKeys.NewMapName));
        }

        public void Open(string dialogTitle, string placeholderMapName, Action onOk = null, Action onCancel = null)
        {
            base.Open(dialogTitle, onOk, onCancel);
            
            mapNameInput.SetPlaceholderText(placeholderMapName);
        }
    }
}
