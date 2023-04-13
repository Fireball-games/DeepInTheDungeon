using System;
using UnityEngine;

namespace Scripts.InventoryManagement.Utils.UI.Dragging
{
    public class DragHelper : MonoBehaviour
    {
        [Tooltip("Size of image when item is dragged out from inventory")]
        [SerializeField] private Vector2 dragSize = new(100, 100);
        [SerializeField] private Vector3 draggedObjectOffset = new(0, 0, 0.7f);
        
        [SerializeField] private float maxDragHeight = 0.2f;
        [SerializeField] private float minDragHeight = -0.4f;
        [SerializeField] private float maxDragWidth = 0.4f;
        [SerializeField] private float minDragWidth = -0.4f;
        [SerializeField] private float throwPower = 7f;
        
        public static Vector2 DragSize { get; private set; }
        public static Vector3 DraggedObjectOffset { get; private set; }
        public static float MaxDragHeight { get; private set; }
        public static float MinDragHeight { get; private set; }
        public static float MaxDragWidth { get; private set; }
        public static float MinDragWidth { get; private set; }
        public static float ThrowPower { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            DragSize = dragSize;
            DraggedObjectOffset = draggedObjectOffset;
            
            MaxDragHeight = maxDragHeight;
            MinDragHeight = minDragHeight;
            MaxDragWidth = maxDragWidth;
            MinDragWidth = minDragWidth;
            
            ThrowPower = throwPower;
        }
#endif
        
        
    }
}