using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalController : MonoBehaviour
{
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

    private void Activate() => SetActive(true);
    private void Deactivate() => SetActive(false);
    private void SetActive(bool isActive) => gameObject.SetActive(isActive);
}
