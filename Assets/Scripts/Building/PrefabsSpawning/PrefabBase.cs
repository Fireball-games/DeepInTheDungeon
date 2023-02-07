using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.Building.PrefabsSpawning
{
    [SelectionBase]
    public abstract class PrefabBase : MonoBehaviour, IPoolInitializable
    {
        public EPrefabType prefabType;
        public string DisplayName => $"{gameObject.name}_{transform.position}";

        [SerializeField] private string guid;

        public string Guid
        {
            get
            {
                if (string.IsNullOrEmpty(guid))
                {
                    guid = global::System.Guid.NewGuid().ToString();
                }

                return guid;
            }
            set => guid = value;
        }

        public virtual void InitializeFromPool()
        {
            transform.position = Vector3.zero;
            guid = global::System.Guid.NewGuid().ToString();
        }
    }
}