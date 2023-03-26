using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement;
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
            _text ??= transform.Find("Text").GetComponent<TMP_Text>();
            
            string prefix = $" {t.GetStatPrefix(modifier.stat).WrapInColor(GetColor(modifier.stat))}:";

            string value = modifier.modifierOperation switch
            {
                ItemModifier.EModifierOperation.Add =>
                    $"{(modifier.value < 0 ? "-" : modifier.value == 0 ? "" : "+")} {modifier.value}"
                        .WrapInColor(modifier.value < 0 ? Colors.Negative :
                            modifier.value == 0 ? Colors.White : Colors.Positive),
                ItemModifier.EModifierOperation.AddRange => $"+ {modifier.valueMin}-{modifier.valueMax}".WrapInColor(
                    Colors.Positive),
                ItemModifier.EModifierOperation.Multiply => $"x {modifier.value}"
                    .WrapInColor(modifier.value < 0 ? Colors.Negative :
                        modifier.value == 0 ? Colors.White : Colors.Positive),
                _ => string.Empty
            };

            _text.text = $"{prefix} {value}";
        }

        private static Color GetColor(ECharacterStats stat) => stat switch
        {
            ECharacterStats.PhysicalDamage => Colors.LightGray,
            ECharacterStats.PhysicalResistance => Colors.LightGray,
            ECharacterStats.FireDamage => Colors.Orange,
            ECharacterStats.FireResistance => Colors.Orange,
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
    }
}