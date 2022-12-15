using System;
using System.Collections.Generic;
using Scripts.Building.Walls.Configurations;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.UI.EditorUI;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.Building.PrefabsSpawning.Walls
{
    public abstract class WallPrefabBase : MonoBehaviour
    {
        public abstract EWallType GetWallType();
        public List<Vector3> waypoints;
        public GameObject PresentedInEditor;

        private MapEditorManager Manager => MapEditorManager.Instance;
        
        private Cursor3D _cursor3D;
        private bool _wallEligibleForEditing;
        private WallConfiguration _ownConfiguration;

        private void OnDisable()
        {
            _ownConfiguration = null;
        }

        private void Update()
        {
            if (IsInEditor())
            {
                if (_wallEligibleForEditing && Input.GetMouseButtonUp(0))
                {
                    var ownTransform = transform;
                    EditorUIManager.Instance.OpenWallEditorWindow(_ownConfiguration);
                }
            }
        }

        private void OnMouseEnter()
        {
            if (IsInEditor())
            {
                Transform ownTransform = transform;
                Vector3 position = ownTransform.position;
                Quaternion rotation = ownTransform.rotation;

                FindObjectOfType<Cursor3D>().ShowAt(position, Cursor3D.EditorWallCursorScale, rotation);
                
                _ownConfiguration ??= Manager.MapBuilder
                    .GetPrefabConfigurationByTransformData(new PositionRotation(position, rotation)) as WallConfiguration;
                _wallEligibleForEditing = true;
            }
        }

        private void OnMouseExit()
        {
            if (IsInEditor())
            {
                EditorUIManager.Instance.WallGizmo.Reset();
                _wallEligibleForEditing = false;
            }
        }

        private bool IsInEditor() =>
            GameManager.Instance.GameMode == GameManager.EGameMode.Editor 
            && MapEditorManager.Instance.WorkMode == EWorkMode.Walls
            && !EditorUIManager.Instance.IsAnyObjectEdited;
    }
}