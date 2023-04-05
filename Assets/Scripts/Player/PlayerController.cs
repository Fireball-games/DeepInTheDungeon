using System;
using Scripts.Helpers;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Saving;

namespace Scripts.Player
{
    public class PlayerController : SingletonNotPersisting<PlayerController>
    {
        public PlayerMovement PlayerMovement { get; private set; }
        public PlayerInventoryManager InventoryManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            PlayerMovement = GetComponent<PlayerMovement>();
            InventoryManager = GetComponentInChildren<PlayerInventoryManager>();
        }

        private void OnEnable()
        {
            InventoryManager.CloseInventories();
        }

        private void OnDisable()
        {
            InventoryManager.ClearInventory();
            InventoryManager.CloseInventories();
        }

        /// <summary>
        /// Used to store player data for saving.
        /// </summary>
        /// <returns></returns>
        public PlayerSaveData CaptureState()
        {
            Logger.Log($"Captured player position: {PlayerMovement.PreviousPosition}");
            return new PlayerSaveData
            {
                currentCampaign = GameManager.Instance.CurrentCampaign.CampaignName,
                currentMap = GameManager.Instance.CurrentMap.MapName,
                playerTransformData = new PositionRotation
                {
                    Position = PlayerMovement.PreviousPosition,
                    Rotation = transform.rotation
                }
            };
        }
    }
}