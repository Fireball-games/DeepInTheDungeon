using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.System;
using Scripts.Triggers;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TriggerEditor : PrefabEditorBase<TriggerConfiguration, Trigger>
    {
        protected override TriggerConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(Placeholder.transform.position, Quaternion.Euler(Vector3.zero)),
            SpawnPrefabOnBuild = true,
            
            Subscribers = new List<string>(),
        };

        protected override TriggerConfiguration CopyConfiguration(TriggerConfiguration sourceConfiguration) => new(sourceConfiguration);
        
        
    }
}