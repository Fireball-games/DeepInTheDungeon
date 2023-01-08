using Scripts.EventsManagement;
using Scripts.MapEditor.Services;
using Scripts.System.MonoBases;
using Scripts.UI.Components;

namespace Scripts.UI.EditorUI.Components
{
    public class VisualControlButtons : UIElementBase
    {
        private ToggleFramedButton _perspectiveToggle;
        private ToggleFramedButton _waypointsShowToggle;

        private void Awake()
        {
            _perspectiveToggle = body.transform.Find("PerspectiveToggle").GetComponent<ToggleFramedButton>();
            _perspectiveToggle.OnClick.AddListener(OnPerspectiveToggleClick);
            _perspectiveToggle.dontToggleOnclick = true;
            _waypointsShowToggle = body.transform.Find("WaypointsShowToggle").GetComponent<ToggleFramedButton>();
            _waypointsShowToggle.OnClick.AddListener(OnWaypointsShowToggleClick);
        }

        private void OnEnable()
        {
            EditorEvents.OnCameraPerspectiveChanged += OnCameraPerspectiveChanged;
        }

        private void OnDisable()
        {
            EditorEvents.OnCameraPerspectiveChanged -= OnCameraPerspectiveChanged;
        }
    
        private void OnCameraPerspectiveChanged(bool isOrthographic)
        {
            if (isOrthographic)
                _perspectiveToggle.ToggleOff(true);
            else
                _perspectiveToggle.ToggleOn(true);
        }

        private void OnPerspectiveToggleClick()
        {
            EditorCameraService.ToggleCameraPerspective();
        }

        private void OnWaypointsShowToggleClick()
        {
            if (_waypointsShowToggle.toggled)
                PathsService.ShowPaths(PathsService.EPathsType.Waypoint);
            else
                PathsService.HidePaths(PathsService.EPathsType.Waypoint);
        }
    }
}
