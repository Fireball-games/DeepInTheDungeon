using Scripts.UI.Components;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI
{
    public class SelectModeExtendedOptions : ExtendedOptionsBase
    {
        private ImageButton _setWallsButton;
        private ImageButton _editEntryPointsButton;

        protected override void Awake()
        {
            DefaultWorkMode = EWorkMode.Triggers;
            AddButtonToMap(ref _setWallsButton, nameof(_setWallsButton), EWorkMode.SetWalls);
            AddButtonToMap(ref _editEntryPointsButton, nameof(_editEntryPointsButton), EWorkMode.EditEntryPoints);
            
            base.Awake();
        }
    }
}