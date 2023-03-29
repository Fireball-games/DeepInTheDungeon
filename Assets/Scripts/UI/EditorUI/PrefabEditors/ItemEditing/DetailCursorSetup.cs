using System;
using DG.Tweening;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors.ItemEditing
{
    [Serializable]
    public struct DetailCursorSetup
    {
        public Sprite Image;
        public Colors.EColor Color;
        public Func<SpriteRenderer, Tween> DetailTweenFunc; 
    }
}