using System;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.UI.Components
{
    public class ScrollViewUIElement : UIElementBase
    {
        [NonSerialized] Transform _content;
        
        public Transform Content => _content ??= transform.Find("Scroll View/Viewport/Content");
    }
}