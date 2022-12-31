using Scripts.EventsManagement;
using Scripts.MapEditor.Services;
using Scripts.System.MonoBases;
using Scripts.UI.Components;

public class VisualControlButtons : UIElementBase
{
    private ToggleFramedButton _perspectiveToggle;
    private ToggleFramedButton _waypointsShowToggle;

    private void Awake()
    {
        _perspectiveToggle = body.transform.Find("PerspectiveToggle").GetComponent<ToggleFramedButton>();
        _waypointsShowToggle = body.transform.Find("WaypointsShowToggle").GetComponent<ToggleFramedButton>();
    }

    private void OnEnable()
    {
        _perspectiveToggle.OnClick += OnPerspectiveToggleClick;
        _waypointsShowToggle.OnClick += OnWaypointsShowToggleClick;
        _perspectiveToggle.dontToggleOnclick = true;
        EditorEvents.OnCameraPerspectiveChanged += OnCameraPerspectiveChanged;
    }

    private void OnDisable()
    {
        _perspectiveToggle.OnClick -= OnPerspectiveToggleClick;
        _waypointsShowToggle.OnClick -= OnWaypointsShowToggleClick;
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
            PathsService.ShowWaypoints();
        else
            PathsService.HideWaypoints();
    }
}
