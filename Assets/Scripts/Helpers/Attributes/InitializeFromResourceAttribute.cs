using System;
using UnityEngine;

namespace Scripts.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssignFromResourceAttribute : Attribute
    {
        public readonly string SourcePath;
        public readonly Type RequiredPrefabType;

        public AssignFromResourceAttribute(string resourcePath = "") : this(resourcePath, typeof(MonoBehaviour))
        {
        }

        public AssignFromResourceAttribute(string resourcePath, Type requiredPrefabType)
        {
            SourcePath = resourcePath;
            RequiredPrefabType = requiredPrefabType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class InstantiateFromResourceAttribute : Attribute
    {
        public readonly string SourcePath;
        public readonly Type RequiredPrefabType;
        public Vector3 Position;
        public readonly bool OverwriteRotation;
        public Vector3 Rotation;
    
        public InstantiateFromResourceAttribute(string resourcePath) : this(resourcePath, typeof(MonoBehaviour), new []{0f, 0f, 0f}, false, new []{0f, 0f, 0f})
        {
        }
    
        public InstantiateFromResourceAttribute(string resourcePath, Type requiredPrefabType) : this(resourcePath, requiredPrefabType, new []{0f, 0f, 0f}, false,new []{0f, 0f, 0f})
        {
        }
    
        public InstantiateFromResourceAttribute(string resourcePath, Type requiredPrefabType, float[] position) : this(resourcePath, requiredPrefabType, position, false, new []{0f, 0f, 0f})
        {
        }
    
        public InstantiateFromResourceAttribute(string resourcePath, Type requiredPrefabType, float[] position, bool overwriteRotation, float[] rotation)
        {
            SourcePath = resourcePath;
            RequiredPrefabType = requiredPrefabType;
            Position = new Vector3(position[0], position[1], position[2]);
            OverwriteRotation = overwriteRotation;
            Rotation = new Vector3(rotation[0], rotation[1], rotation[2]);
        }
    }
}