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
        [SerializeField] float effectDuration = 3f;
        [SerializeField] float scaleDuration = 1f;
        [SerializeField] Material idleMaterial;
        private MeshRenderer _meshRenderer;
        private GameObject _innerCube;
        private ParticleSystem _centerEffect;
        private Sequence _dissolveSequence;
        private Sequence _solidifySequence;
        private Material _dissolvingMaterial;

        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
    
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;

        private bool _isWorking;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _dissolvingMaterial = new Material(_meshRenderer.material);
            _meshRenderer.material = idleMaterial;

            _innerCube = transform.GetChild(0).gameObject;
            _innerCube.SetActive(false);

            _centerEffect = transform.Find("CenterEffect").GetComponent<ParticleSystem>();
            _centerEffect.Stop();

            _dissolveSequence =
                DOTween.Sequence(_meshRenderer.material.DOFloat(0.85f, "_Dissolve", effectDuration));
            _dissolveSequence.Insert(0, _innerCube.transform.DOScale(0, scaleDuration).SetDelay(effectDuration - scaleDuration));
            _dissolveSequence.Insert(0,
                _innerCube.transform.DOLocalRotate(Quaternion.Euler(1800, 1800, 0).eulerAngles, scaleDuration)
                    .SetDelay(effectDuration - scaleDuration));
            _dissolveSequence.SetAutoKill(false);
        }

        private async Task StartDissolve()
        {
            if (_isWorking) return;

            _meshRenderer.material = _dissolvingMaterial;
            _innerCube.SetActive(true);
            _centerEffect.gameObject.SetActive(true);
            _centerEffect.Play();

            TaskCompletionSource<bool> tsc = new();
            _meshRenderer.material.DOFloat(0.85f, Dissolve, effectDuration - scaleDuration).OnComplete(() =>
            {
                MapBuilder.SetTileForMovement(transform.position, true);
            
                _innerCube.transform.DOScale(0.01f, scaleDuration).SetEase(Ease.OutExpo).OnComplete(() =>
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

            _innerCube.transform.DOScale(0.999f, scaleDuration).SetEase(Ease.InExpo).OnComplete(() =>
            {
                _meshRenderer.material.DOFloat(0, Dissolve, effectDuration).OnComplete(() =>
                {
                    _meshRenderer.material = idleMaterial;
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