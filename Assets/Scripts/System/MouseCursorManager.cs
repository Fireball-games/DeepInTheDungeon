using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.MapEditor;
using Scripts.UI.EditorUI;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System
{
    public class MouseCursorManager : MonoBehaviour
    {
        [SerializeField] private List<MouseCursorSetup> cursorSetups;
        
        private static bool _isDefaultCursorSet = true;

        private static EditorUIManager UIManager => EditorUIManager.Instance;
        private static Cursor3D _cursor3D;
        private static Cursor3D Cursor3D => GetCursor3D();
        private static Cursor3D GetCursor3D()  
        {
            if(!_cursor3D)
            {
                _cursor3D = FindObjectOfType<Cursor3D>();
            }

            return _cursor3D;
        }

        private static Dictionary<ECursorType, MouseCursorSetup> _cursorMap;

        public enum ECursorType
        {
            Default = 0,
            Build = 1,
            Demolish = 2,
            Move = 3,
            Edit = 4,
            Add = 5,
        }

        public enum EHotspotType
        {
            Default = 0,
            Middle = 1,
        }

        private void Awake()
        {
            _cursorMap = cursorSetups.ToDictionary(s => s.type);
        }

        public static void SetDefaultCursor()
        {
            if (_isDefaultCursorSet) return;

            Hide3DCursor();
            
            SetCursor(null, Vector3.zero);
            _isDefaultCursorSet = true;
        }

        public static void ResetCursor()
        {
            Hide3DCursor();
            SetDefaultCursor();
        }
        
        public static void SetCursor(ECursorType type)
        {
            if (type is ECursorType.Default)
            {
                SetDefaultCursor();
                return;
            }
            
            if(!_cursorMap.TryGetValue(type, out MouseCursorSetup setup))
            {
                Logger.Log($"No registered setup for type {type} was found, add one");
                SetDefaultCursor();
                return;
            }

            Texture2D texture = setup.texture;
            Vector2 hotspot = setup.hotspotType switch
            {
                EHotspotType.Default => Vector2.zero,
                EHotspotType.Middle => new Vector2(texture.height / 2, texture.width / 2),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            SetCursor(texture, hotspot);
        }
        
        private static void SetCursor(Texture2D image, Vector3 hotspot)
        {
            Cursor.SetCursor(image, hotspot, CursorMode.Auto);
            _isDefaultCursorSet = false;
        }
        
        public static void Hide3DCursor()
        {
            if (!UIManager.IsAnyObjectEdited)
            {
                Cursor3D.Hide();
            }
        }

        [Serializable]
        public class MouseCursorSetup
        {
            public ECursorType type;
            public Texture2D texture;
            public EHotspotType hotspotType;
        } 
    }
}