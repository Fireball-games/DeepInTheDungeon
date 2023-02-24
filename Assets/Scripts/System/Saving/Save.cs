using System;
using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.Player.CharacterSystem;
using UnityEngine;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Class describing all data for saved position, character, campaigns states, current campaign/map and player position in that map.
    /// </summary>
    [Serializable]
    public class Save
    {
        public string saveName;
        public CharacterProfile characterProfile;
        public PlayerSaveData playerSaveData;
        public List<CampaignSave> campaignsSaves;

        public DateTime timeSaved;
        public byte[] screenshot;
        
        public Vector3 PlayerGridPosition => playerSaveData.playerTransformData.Position.ToGridPosition();
        public Quaternion PlayerRotation => playerSaveData.playerTransformData.Rotation;
        public string CurrentCampaign => playerSaveData.currentCampaign;
        public string CurrentMap => playerSaveData.currentMap;
    }
}