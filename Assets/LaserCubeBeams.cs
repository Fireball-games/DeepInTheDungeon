using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LaserCubeBeams : MonoBehaviour
{
    [SerializeField] float _effectDuration = 3f;
    [SerializeField] float _scaleDuration = 1f;
    private List<LaserBeamController> _beamControllers = new();
    private GameObject _innerCube;

    private void Awake()
    {
        _beamControllers = GetComponentsInChildren<LaserBeamController>().ToList();
        _innerCube = transform.GetChild(0).gameObject;
    }

    private void StartDissolve()
    {
        _innerCube.SetActive(true);

        foreach (var beam in _beamControllers)
        {
            beam.ActivateBeam(_effectDuration);
        }

        GetComponent<MeshRenderer>().material.DOFloat(0.85f, "_Dissolve", _effectDuration - _scaleDuration).OnComplete(() =>
        {
            _innerCube.transform.DOScale(0.01f, _scaleDuration).OnComplete(() =>
            {
                foreach (var beam in _beamControllers)
                {
                    beam.DeactivateBeam();
                }
                _innerCube.SetActive(false);
            }).SetAutoKill(true).Play();
        }).SetAutoKill(true).Play();
    }

    private void Solidify()
    {
        _innerCube.SetActive(true);
        
        foreach (var beam in _beamControllers)
        {
            beam.ActivateBeam(_effectDuration);
        }

        _innerCube.transform.DOScale(0.999f, _scaleDuration).OnComplete(() =>
        {
            GetComponent<MeshRenderer>().material.DOFloat(0f, "_Dissolve", _effectDuration - _scaleDuration).OnComplete(() =>
                {
                    foreach (var beam in _beamControllers)
                    {
                        beam.DeactivateBeam();
                    }
                    _innerCube.SetActive(false);
                }).SetAutoKill(true).Play();
        }).SetAutoKill(true).Play();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Start Dissolve"))
        {
            StartDissolve();
        }

        if (GUILayout.Button("Solidify"))
        {
            Solidify();
        }
    }
}