using UnityEngine;
using UnityEngine.UI;

public class ModalController : MonoBehaviour
{
    [SerializeField] private Button body;

    private void Awake()
    {
        body.onClick.AddListener(OnModalClicked);
    }

    private void OnEnable()
    {
        EventsManager.OnModalShowRequested += Activate;
        EventsManager.OnModalHideRequested += Deactivate;
    }

    private void OnDisable()
    {
        EventsManager.OnModalShowRequested -= Activate;
        EventsManager.OnModalHideRequested -= Deactivate;
    }

    private void OnModalClicked() => EventsManager.TriggerOnModalClicked();

    private void Activate() => SetActive(true);
    private void Deactivate() => SetActive(false);
    private void SetActive(bool isActive) => body.gameObject.SetActive(isActive);
}
