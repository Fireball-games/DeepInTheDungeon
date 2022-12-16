using System;
using System.Collections.Generic;
using Scripts.Building.Walls.Configurations;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : MonoBehaviour, IPoolInitializable
    {
        public abstract EWallType GetWallType();
        public List<Vector3> waypoints;
        public GameObject PresentedInEditor;

        private MapEditorManager Manager => MapEditorManager.Instance;
        
        private Cursor3D _cursor3D;
        [NonSerialized] public bool WallEligibleForEditing;
        private WallConfiguration _ownConfiguration;

        public void Initialize()
        {
            WallEligibleForEditing = false;
        }
        
        private void OnDisable()
        {
            _ownConfiguration = null;
        }

        private void Update()
        {
            if (!IsInEditor()) return;

            if (WallEligibleForEditing && Input.GetMouseButtonUp(0))
            {
                EditorUIManager.Instance.OpenWallEditorWindow(_ownConfiguration);
                WallEligibleForEditing = false;
            }
        }

        private bool IsInEditor() =>
            GameManager.Instance.GameMode == GameManager.EGameMode.Editor 
            && MapEditorManager.Instance.WorkMode == EWorkMode.Walls
            && !EditorUIManager.Instance.IsAnyObjectEdited;

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