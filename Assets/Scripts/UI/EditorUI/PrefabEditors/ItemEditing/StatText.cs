using Scripts.Helpers;
using Scripts.Inventory;
using Scripts.Localization;
using TMPro;
using UnityEngine;
using static Scripts.Player.CharacterSystem.CharacterProfile;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    public class StatText : MonoBehaviour
    {
        private TMP_Text _text;
        
        public void Set(ItemModifier modifier)
        {
            _text ??= GetComponent<TMP_Text>();
            
            string prefix = $"{t.GetStatPrefix(modifier.stat)}: ";
            
            Color color = modifier.stat switch
            {
                ECharacterStats.PhysicalDamage => Colors.LightGray,
                ECharacterStats.PhysicalResistance => Colors.LightGray,
                ECharacterStats.FireDamage => Colors.DarkRed,
                ECharacterStats.FireResistance => Colors.DarkRed,
                ECharacterStats.IceDamage => Colors.DeepBlue,
                ECharacterStats.IceResistance => Colors.DeepBlue,
                ECharacterStats.LightningDamage => Colors.LightYellow,
                ECharacterStats.LightningResistance => Colors.LightYellow,
                ECharacterStats.PoisonDamage => Colors.DarkGreen,
                ECharacterStats.PoisonResistance => Colors.DarkGreen,
                ECharacterStats.MagicDamage => Colors.LightBlue,
                ECharacterStats.MagicResistance => Colors.LightBlue,
                ECharacterStats.Health => Colors.LightRed,
                ECharacterStats.Mana => Colors.LightBlue,
                _ => Color.white
            };
            
            string value = modifier.modifierOperation switch
            {
                ItemModifier.EModifierOperation.Add => $"{(modifier.value < 0 ? "-" : modifier.value == 0 ? "" : "+")}{modifier.value}",
                ItemModifier.EModifierOperation.AddRange => $"+{modifier.valueMin}-{modifier.valueMax}",
                ItemModifier.EModifierOperation.Multiply => $"x{modifier.value}",
                _ => string.Empty
            };
        }
    }
}