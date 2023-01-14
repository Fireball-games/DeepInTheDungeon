﻿using System;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Building.Walls
{
    [SelectionBase]
    public abstract class PrefabBase : MonoBehaviour, IPoolInitializable
    {
        public EPrefabType prefabType;
        public string DisplayName => $"{gameObject.name}_{transform.position}";

        [SerializeField] private string guid;

        // ReSharper disable once InconsistentNaming
        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(guid))
                {
                    guid = Guid.NewGuid().ToString();
                }

                return guid;
            }
            set => guid = value;
        }

        public virtual void InitializeFromPool()
        {
            transform.position = Vector3.zero;
            guid = Guid.NewGuid().ToString();
        }
    }
}