using System.Collections.Generic;
using UnityEngine;

public class IconStore : MonoBehaviour
{
    [SerializeField] private Sprite move;
    [SerializeField] private Sprite exclamation;
    [SerializeField] private Sprite triggerReceiver;
    [SerializeField] private Sprite wall;

    private static Dictionary<EIcon, Sprite> _sprites;

    public enum EIcon
    {
        None = 0,
        Move = 1,
        Exclamation = 2,
        Trigger = 2,
        TriggerReceiver = 3,
        Wall = 4,
    }

    private void Awake()
    {
        _sprites = new Dictionary<EIcon, Sprite>
        {
            {EIcon.None, null},
            {EIcon.Move, wall},
            {EIcon.Exclamation, exclamation},
            {EIcon.TriggerReceiver, triggerReceiver},
            {EIcon.Wall, wall},
        };
    }

    public static Sprite Get(EIcon icon) => _sprites[icon];
}
