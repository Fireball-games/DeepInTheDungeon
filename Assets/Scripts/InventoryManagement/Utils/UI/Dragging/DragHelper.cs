using System;
using UnityEngine;

namespace Scripts.InventoryManagement.Utils.UI.Dragging
{
    public class DragHelper : MonoBehaviour
    {
        [Tooltip("Size of image when item is dragged out from inventory")]
        [SerializeField] Vector2 dragSize = new(100, 100);
        
        [SerializeField] float maxDragHeight = 0.2f;
        [SerializeField] float minDragHeight = -0.4f;
        [SerializeField] float maxDragWidth = 0.4f;
        [SerializeField] float minDragWidth = -0.4f;
        [SerializeField] float throwPower = 7f;
        
        public static Vector2 DragSize { get; private set; }
        public static float MaxDragHeight { get; private set; }
        public static float MinDragHeight { get; private set; }
        public static float MaxDragWidth { get; private set; }
        public static float MinDragWidth { get; private set; }
        public static float ThrowPower { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            DragSize = dragSize;
            
            MaxDragHeight = maxDragHeight;
            MinDragHeight = minDragHeight;
            MaxDragWidth = maxDragWidth;
            MinDragWidth = minDragWidth;
            
            ThrowPower = throwPower;
        }
#endif
        
        
    }
}