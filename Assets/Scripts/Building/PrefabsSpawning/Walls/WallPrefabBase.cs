using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI;
using UnityEngine;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : PrefabBase, IPoolInitializable
    {
        public List<Vector3> waypoints;
        public GameObject PresentedInEditor;

        private MapEditorManager Manager => MapEditorManager.Instance;
        
        private Cursor3D _cursor3D;
        [NonSerialized] public bool WallEligibleForEditing;
        private WallConfiguration _ownConfiguration;

        public void Initialize()
        {
            WallEligibleForEditing = false;
            _ownConfiguration = null;
        }

        public void OnClickInEditor()
        {
            EditorUIManager.Instance.OpenEditorWindow(_ownConfiguration);
            WallEligibleForEditing = false;
        }

        /// <summary>
        /// Editor uses this method for editing existing walls in editor. 
        /// </summary>
        public void OnMouseEntered()
        {
            Transform ownTransform = transform;
            Vector3 position = ownTransform.position;
            Quaternion rotation = ownTransform.rotation;

            FindObjectOfType<Cursor3D>().ShowAt(position, Cursor3D.EditorWallCursorScale, rotation);
                
            _ownConfiguration ??= Manager.MapBuilder
                .GetPrefabConfigurationByTransformData(new PositionRotation(position, rotation)) as WallConfiguration;
            WallEligibleForEditing = true;
        }
    }
}