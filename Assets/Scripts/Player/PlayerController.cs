using Scripts.Helpers;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Saving;

namespace Scripts.Player
{
    public class PlayerController : SingletonNotPersisting<PlayerController>
    {
        public PlayerMovement PlayerMovement => _playerMovement;
        private PlayerMovement _playerMovement;

        protected override void Awake()
        {
            base.Awake();
            
            _playerMovement = GetComponent<PlayerMovement>();
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