using System;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Character profile agnostic player data. Used with ISavable.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public string currentCampaign;
        public string currentMap;
        public PositionRotation playerTransformData;
    }
}