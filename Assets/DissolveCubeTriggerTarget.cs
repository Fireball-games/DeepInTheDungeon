using System.Threading.Tasks;
using DG.Tweening;
using Scripts.Triggers;
using UnityEngine;

public class DissolveCubeTriggerTarget : StateTriggerTarget
{
    [SerializeField] float _effectDuration = 3f;
    [SerializeField] float _scaleDuration = 1f;
    private MeshRenderer _meshRenderer;
    private GameObject _innerCube;
    private ParticleSystem _centerEffect;
    private Sequence _dissolveSequence;
    private Sequence _solidifySequence;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _innerCube = transform.GetChild(0).gameObject;
        _innerCube.SetActive(false);
        _centerEffect = transform.Find("CenterEffect").GetComponent<ParticleSystem>();
        _centerEffect.Stop();

        _dissolveSequence =
            DOTween.Sequence(_meshRenderer.material.DOFloat(0.85f, "_Dissolve", _effectDuration));
        _dissolveSequence.Insert(0, _innerCube.transform.DOScale(0, _scaleDuration).SetDelay(_effectDuration - _scaleDuration));
        _dissolveSequence.Insert(0, _innerCube.transform.DOLocalRotate(Quaternion.Euler(1800, 1800, 0).eulerAngles, _scaleDuration).SetDelay(_effectDuration - _scaleDuration));
        _dissolveSequence.SetAutoKill(false);
    }

    private async Task<bool> StartDissolve()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();
        _centerEffect.Stop();

        TaskCompletionSource<bool> tsc = new();
        _meshRenderer.material.DOFloat(0.85f, "_Dissolve", _effectDuration - _scaleDuration).OnComplete(() =>
        {
            _innerCube.transform.DOScale(0.01f, _scaleDuration).OnComplete(() =>
            {
                _innerCube.SetActive(false);
                _centerEffect.Stop();
                tsc.SetResult(true);
            }).SetAutoKill(true).Play();
        }).SetAutoKill(true).Play();
        
        return await tsc.Task;
    }

    private async Task<bool> Solidify()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();
        _centerEffect.Stop();
        
        TaskCompletionSource<bool> tsc = new();

        _innerCube.transform.DOScale(0.999f, _scaleDuration).OnComplete(() =>
        {
            _meshRenderer.material.DOFloat(0f, "_Dissolve", _effectDuration - _scaleDuration).OnComplete(() =>
                {
                    _innerCube.SetActive(false);
                    _centerEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    tsc.SetResult(true);
                }).SetAutoKill(true).Play();
        }).SetAutoKill(true).Play();
        
        return await tsc.Task;
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

    protected override async Task RunOnState()
    {
        await StartDissolve();
    }

    protected override async Task RunOffState()
    {
        await Solidify();
    }
}