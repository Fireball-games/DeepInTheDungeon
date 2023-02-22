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
    private ParticleSystem _centerEffect;

    private void Awake()
    {
        _beamControllers = GetComponentsInChildren<LaserBeamController>(true).ToList();
        _innerCube = transform.GetChild(0).gameObject;
        _innerCube.SetActive(false);
        _centerEffect = transform.Find("CenterEffect").GetComponent<ParticleSystem>();
        _centerEffect.Stop();
    }

    private void StartDissolve()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();

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
                _centerEffect.Stop();
            }).SetAutoKill(true).Play();
        }).SetAutoKill(true).Play();
    }

    private void Solidify()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();
        
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
                    _centerEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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