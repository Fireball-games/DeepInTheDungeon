using System;

namespace Scripts.Player.CharacterSystem
{
    /// <summary>
    /// In this class are stored information about the character profile. Statistics, name, image index etc.
    /// </summary>
    [Serializable]
    public class CharacterProfile
    {
        public string name;
        public int imageIndex;
    }
}