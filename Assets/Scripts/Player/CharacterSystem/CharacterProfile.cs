using System;
using UnityEngine;

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
        public string guid;
        public Vector2 inventoryPosition;

        public CharacterProfile()
        {
            guid = Guid.NewGuid().ToString();
            inventoryPosition = Vector2.zero;
        }

        public enum ECharacterStats
        {
            None = 0,
            Agility = 1,
            AlchemistLevel = 2,
            Armor = 3,
            AttackSpeed = 5,
            CriticalChance = 6,
            CriticalDamage = 7,
            FireDamage = 4,
            FireResistance = 9,
            Health = 10,
            IceDamage = 25,
            IceResistance = 11,
            Intelligence = 12,
            LightningDamage = 26,
            LightningResistance = 13,
            LockPicking = 14,
            Luck = 15,
            MageLevel = 16,
            MagicDamage = 27,
            MagicResistance = 17,
            Mana = 18,
            PhysicalDamage = 29,
            PhysicalResistance = 19,
            PoisonDamage = 28,
            PoisonResistance = 20,
            RogueLevel = 21,
            Strength = 22,
            Vitality = 23,
            WarriorLevel = 24,
        }
    }
}