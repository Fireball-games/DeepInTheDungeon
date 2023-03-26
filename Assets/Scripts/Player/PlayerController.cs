using Scripts.Helpers;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Player
{
    public class PlayerController : SingletonNotPersisting<PlayerController>
    {
        [SerializeField] private GameObject pickupColliderPrefab;
        public PlayerMovement PlayerMovement => _playerMovement;
        private PlayerMovement _playerMovement;

        public PlayerInventoryManager InventoryManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            _playerMovement = GetComponent<PlayerMovement>();
            InventoryManager = GetComponentInChildren<PlayerInventoryManager>();
        }

        private void OnEnable()
        {
            pickupColliderPrefab = pickupColliderPrefab.GetFromPool(null);
            pickupColliderPrefab.transform.SetParent(null);
            pickupColliderPrefab.GetComponent<Follow>().target = transform;
        }

        private void OnDisable()
        {
            if (pickupColliderPrefab)
            {
                pickupColliderPrefab.gameObject.DismissToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Logger.Log($"Player entered trigger: {other.name}");
        }

        /// <summary>
        /// Used to store player data for saving.
        /// </summary>
        /// <returns></returns>
        public PlayerSaveData CaptureState()
        {
            Logger.Log($"Captured player position: {_playerMovement.PreviousPosition}");
            return new PlayerSaveData
            {
                currentCampaign = GameManager.Instance.CurrentCampaign.CampaignName,
                currentMap = GameManager.Instance.CurrentMap.MapName,
                playerTransformData = new PositionRotation
                {
                    Position = _playerMovement.PreviousPosition,
                    Rotation = transform.rotation
                }
            };
        }
    }
}