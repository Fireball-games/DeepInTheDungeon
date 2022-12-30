using System;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Building.Walls
{
    public abstract class PrefabBase : MonoBehaviour, IPoolInitializable
    {
        public EPrefabType prefabType;
        public string DisplayName => $"{gameObject.name}_{transform.position}";

        private string _guid;

        // ReSharper disable once InconsistentNaming
        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    _guid = Guid.NewGuid().ToString();
                }

                return _guid;
            }
        }

        public virtual void Initialize()
        {
            _guid = Guid.NewGuid().ToString();
        }
    }
}