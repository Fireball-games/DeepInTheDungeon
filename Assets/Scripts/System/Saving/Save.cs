using System;
using System.Collections.Generic;
using Scripts.Player.CharacterSystem;

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
    }
}