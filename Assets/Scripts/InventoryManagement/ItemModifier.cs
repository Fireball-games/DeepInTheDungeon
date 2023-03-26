using System;
using NaughtyAttributes;
using Scripts.Player.CharacterSystem;

namespace Scripts.InventoryManagement
{
    [Serializable]
    public class ItemModifier
    {
        public EModifierOperation modifierOperation = EModifierOperation.None;
        public CharacterProfile.ECharacterStats stat = CharacterProfile.ECharacterStats.None;
        [HideIf(nameof(modifierOperation), EModifierOperation.AddRange), AllowNesting] public float value;
        [ShowIf(nameof(modifierOperation), EModifierOperation.AddRange), AllowNesting] public float valueMin;
        [ShowIf(nameof(modifierOperation), EModifierOperation.AddRange), AllowNesting] public float valueMax;
        
        public enum EModifierOperation
        {
            None = 0,
            Add = 1,
            AddRange = 2,
            Multiply = 3,
        }
        
        public enum EModifierType
        {
            None = 0,
            Percent = 1,
            Value = 2,
        }
    }
}