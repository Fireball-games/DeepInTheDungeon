using System.Linq;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerController : SingletonNotPersisting<PlayerController>
    {
        public PlayerMovement PlayerMovement { get; private set; }
        public PlayerInventoryManager InventoryManager { get; private set; }
        /// <summary>
        /// Multipurpose plane in front of the player
        /// </summary>
        public GameObject FrontalPlane { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            PlayerMovement = GetComponent<PlayerMovement>();
            InventoryManager = GetComponentInChildren<PlayerInventoryManager>();
            FrontalPlane = transform.Find("FrontalPlane").gameObject;
        }

        private void OnEnable()
        {
            InventoryManager.Initialize();
            InventoryManager.CloseInventories();
            FrontalPlane.SetActive(false);
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
            // Logger.Log($"Captured player position: {PlayerMovement.PreviousPosition}");
            return new PlayerSaveData
            {
                currentCampaign = GameManager.Instance.CurrentCampaign.CampaignName,
                currentMap = GameManager.Instance.CurrentMap.MapName,
                playerTransformData = new PositionRotation
                {
                    Position = PlayerMovement.PreviousPosition,
                    Rotation = transform.rotation
                },
                inventoriesContent = InventoryManager.GetInventorySavables().Select(SaveManager.CaptureSavaData).ToList(),
            };
        }
        
        /// <summary>
        /// Sets active state of the frontal plane.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetFrontalPlaneActive(bool isActive)
        {
            FrontalPlane.SetActive(isActive);
        }
    }
}