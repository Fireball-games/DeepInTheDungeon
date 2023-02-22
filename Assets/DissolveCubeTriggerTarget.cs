using System;
using System.Threading.Tasks;
using DG.Tweening;
using Scripts.Helpers.Extensions;
using Scripts.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class DissolveCubeTriggerTarget : StateTriggerTarget
{
    [SerializeField] float _effectDuration = 3f;
    [SerializeField] float _scaleDuration = 1f;
    private MeshRenderer _meshRenderer;
    private GameObject _innerCube;
    private ParticleSystem _centerEffect;
    private Sequence _dissolveSequence;
    private Sequence _solidifySequence;

    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = new(_meshRenderer.material);

        _innerCube = transform.GetChild(0).gameObject;
        _innerCube.SetActive(false);

        _centerEffect = transform.Find("CenterEffect").GetComponent<ParticleSystem>();
        _centerEffect.Stop();

        _materialPropertyBlock = new MaterialPropertyBlock();

        _dissolveSequence =
            DOTween.Sequence(_meshRenderer.material.DOFloat(0.85f, "_Dissolve", _effectDuration));
        _dissolveSequence.Insert(0, _innerCube.transform.DOScale(0, _scaleDuration).SetDelay(_effectDuration - _scaleDuration));
        _dissolveSequence.Insert(0,
            _innerCube.transform.DOLocalRotate(Quaternion.Euler(1800, 1800, 0).eulerAngles, _scaleDuration)
                .SetDelay(_effectDuration - _scaleDuration));
        _dissolveSequence.SetAutoKill(false);
    }

    private async Task<bool> StartDissolve()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();
        _centerEffect.Stop();

        TaskCompletionSource<bool> tsc = new();
        await SetDissolveValue(0.85f, true);
        
        _innerCube.transform.DOScale(0.01f, _scaleDuration).OnComplete(() =>
        {
            _innerCube.SetActive(false);
            tsc.SetResult(true);
        }).SetAutoKill(true).Play();

        return await tsc.Task;
    }

    private async Task<bool> Solidify()
    {
        _innerCube.SetActive(true);
        _centerEffect.Play();
        
        Task.Delay(1).ContinueWith(_ =>_centerEffect.Stop());

        TaskCompletionSource<bool> tsc = new();

        _innerCube.transform.DOScale(0.999f, _scaleDuration).OnComplete(() =>
        {
            SetDissolveValue(0, false).ContinueWith(_ =>
            {
                _innerCube.SetActive(false);
                tsc.SetResult(true);
            });
        }).SetAutoKill(true).Play();
        
        return await tsc.Task;
    }
    
    private async Task SetDissolveValue(float targetValue, bool isDissolving)
    {
        float originalValue = _meshRenderer.material.GetFloat(Dissolve);
        float currentDissolveValue = originalValue;
        float startTime = Time.time;

        bool Predicament() => isDissolving 
            ? currentDissolveValue <= targetValue 
            : currentDissolveValue >= targetValue;
        
        float ValueLerp() => isDissolving 
            ? Mathf.InverseLerp(0, _effectDuration, Time.time - startTime) 
            : Mathf.InverseLerp(_effectDuration, 0, Time.time - startTime);
        
        while(Predicament())
        {
            currentDissolveValue = ValueLerp();
            _materialPropertyBlock.SetFloat(Dissolve, currentDissolveValue);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
            await Task.Yield();
        }
    }

#if UNITY_EDITOR
    private async void OnGUI()
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
#endif

    protected override async Task RunOnState() => await StartDissolve();

    protected override async Task RunOffState() => await Solidify();

    public override void SetState(int state)
    {
        if (state == 0)
        {
            _innerCube.SetActive(true);
            _innerCube.transform.localScale = 0.999f.ToVectorUniform();
            _meshRenderer.material.SetFloat(Dissolve, 0f);
        }
        else
        {
            _innerCube.SetActive(false);
            _innerCube.transform.localScale = 0f.ToVectorUniform();
            _meshRenderer.material.SetFloat(Dissolve, 1f);
        }
    }
}