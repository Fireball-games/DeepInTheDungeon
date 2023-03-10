using System.Threading.Tasks;
using DG.Tweening;
using Scripts.Building;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;

namespace Scripts.Triggers
{
    public class DissolveCubeTriggerTarget : StateTriggerTarget
    {
        [SerializeField] float _effectDuration = 3f;
        [SerializeField] float _scaleDuration = 1f;
        private MeshRenderer _meshRenderer;
        private GameObject _innerCube;
        private ParticleSystem _centerEffect;
        private Sequence _dissolveSequence;
        private Sequence _solidifySequence;

        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
    
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;

        private bool _isWorking;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.material = new Material(_meshRenderer.material);

            _innerCube = transform.GetChild(0).gameObject;
            _innerCube.SetActive(false);

            _centerEffect = transform.Find("CenterEffect").GetComponent<ParticleSystem>();
            _centerEffect.Stop();

            _dissolveSequence =
                DOTween.Sequence(_meshRenderer.material.DOFloat(0.85f, "_Dissolve", _effectDuration));
            _dissolveSequence.Insert(0, _innerCube.transform.DOScale(0, _scaleDuration).SetDelay(_effectDuration - _scaleDuration));
            _dissolveSequence.Insert(0,
                _innerCube.transform.DOLocalRotate(Quaternion.Euler(1800, 1800, 0).eulerAngles, _scaleDuration)
                    .SetDelay(_effectDuration - _scaleDuration));
            _dissolveSequence.SetAutoKill(false);
        }

        private async Task StartDissolve()
        {
            if (_isWorking) return;

            _innerCube.SetActive(true);
            _centerEffect.gameObject.SetActive(true);
            _centerEffect.Play();

            TaskCompletionSource<bool> tsc = new();
            _meshRenderer.material.DOFloat(0.85f, Dissolve, _effectDuration - _scaleDuration).OnComplete(() =>
            {
                MapBuilder.SetTileForMovement(transform.position, true);
            
                _innerCube.transform.DOScale(0.01f, _scaleDuration).SetEase(Ease.OutExpo).OnComplete(() =>
                {
                    _innerCube.SetActive(false);
                    tsc.SetResult(true);
                }).SetAutoKill(true).Play();
            }).SetAutoKill(true).Play();
        
            await tsc.Task;
        }

        private async Task Solidify()
        {
            if (_isWorking) return;

            _innerCube.SetActive(true);
            _centerEffect.gameObject.SetActive(true);
            _centerEffect.Play();
        
            MapBuilder.SetTileForMovement(transform.position, false);

            TaskCompletionSource<bool> tsc = new();

            _innerCube.transform.DOScale(0.999f, _scaleDuration).SetEase(Ease.InExpo).OnComplete(() =>
            {
                _meshRenderer.material.DOFloat(0, Dissolve, _effectDuration).OnComplete(() =>
                {
                    _innerCube.SetActive(false);
                    tsc.SetResult(true);
                }).SetAutoKill(true).Play();
            }).SetAutoKill(true).Play();

            await tsc.Task;
        }

        protected override async Task RunOnState() => await StartDissolve();

        protected override async Task RunOffState() => await Solidify();

        public override void SetState(int state)
        {
            // Solid
            if (state == 0)
            {
                _innerCube.SetActive(true);
                _innerCube.transform.localScale = 0.999f.ToVectorUniform();
                _meshRenderer.material.SetFloat(Dissolve, 0f);
            }
            // Dissolved
            else
            {
                _innerCube.SetActive(false);
                _innerCube.transform.localScale = 0f.ToVectorUniform();
                _meshRenderer.material.SetFloat(Dissolve, 0.85f);
            }
        
            MapBuilder.SetTileForMovement(transform.position, state == 1);
        }
    }
}