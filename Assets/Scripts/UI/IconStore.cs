using System.Collections.Generic;
using UnityEngine;

namespace Scripts.UI
{
    public class IconStore : MonoBehaviour
    {
        [SerializeField] private Sprite move;
        [SerializeField] private Sprite exclamation;
        [SerializeField] private Sprite triggerReceiver;
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite embedded;

        private static Dictionary<EIcon, Sprite> _sprites;

        public enum EIcon
        {
            None = 0,
            Move = 1,
            Exclamation = 2,
            Trigger = 2,
            TriggerReceiver = 3,
            Wall = 4,
            Embedded = 5,
        }

        private void Awake()
        {
            _sprites = new Dictionary<EIcon, Sprite>
            {
                {EIcon.None, null},
                {EIcon.Move, move},
                {EIcon.Exclamation, exclamation},
                {EIcon.TriggerReceiver, triggerReceiver},
                {EIcon.Wall, wall},
                {EIcon.Embedded, embedded}
            };
        }

        public static Sprite Get(EIcon icon) => _sprites[icon];
    }
}
