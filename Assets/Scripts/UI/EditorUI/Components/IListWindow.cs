using Scripts.System;

namespace Scripts.UI.EditorUI.Components
{
    public interface IListWindow
    {
        public PositionRotation OriginalCameraPositionRotation { get; set; }
        public int OriginalFloor { get; set; }
        public void SetNavigatedAway();
    }
}