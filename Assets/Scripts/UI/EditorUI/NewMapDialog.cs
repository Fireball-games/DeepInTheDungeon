using System.Linq;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Helpers;
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
            rowsInput.SetLabelText(t.Get(Keys.Rows));
            columnsInput.SetInputText(columnsCount.ToString());
            columnsInput.SetLabelText(t.Get(Keys.Columns));
            floorsInput.SetInputText(floorsCount.ToString());
            floorsInput.SetLabelText(t.Get(Keys.Floors));
            mapNameInput.SetPlaceholderText(t.Get(Keys.NewMapNamePrompt));
            mapNameInput.SetLabelText(t.Get(Keys.NewMapName));
        }

        public async Task<EConfirmResult> Show(Campaign parentCampaign)
        {
            string defaultMapName = Strings.GetDefaultName(
                t.Get(Keys.NewMapName),
                parentCampaign.Maps.Select(m => m.MapName));
            
            mapNameInput.SetInputText("");
            mapNameInput.SetPlaceholderText(defaultMapName);

            return await base.Show(t.Get(Keys.NewMap),  t.Get(Keys.CreateMap));
        }
    }
}
