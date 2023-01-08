using System.Collections.Generic;
using Scripts.Helpers;
using TMPro;
using UnityEngine;

//Fireball Games * * * PetrZavodny.com

namespace Scripts.UI.Components
{
    public class StatusBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject body;

        private Color NormalColor => Colors.White;
        private Color PositiveColor => Colors.Positive;
        private Color WarningColor => Colors.Warning;
        private Color NegativeColor => Colors.Negative;
        
        private bool _isBusy;
        private readonly Queue<KeyValuePair<string, EMessageType>> _messageQueue = new();
        private Dictionary<EMessageType, Color> _colorMap;

        public enum EMessageType
        {
            None = 0,
            Positive = 1,
            Negative = 2,
            Warning = 3
        }

        private void Awake()
        {
            _colorMap = new()
            {
                {EMessageType.None, NormalColor},
                {EMessageType.Positive, PositiveColor},
                {EMessageType.Warning, WarningColor},
                {EMessageType.Negative, NegativeColor},
            };
            
            body.SetActive(false);
        }

        public void RegisterMessage(string text, EMessageType messageType)
        {
            _messageQueue.Enqueue(new KeyValuePair<string, EMessageType>(text, messageType));

            if (!_isBusy)
            {
                DisplayMessage();
            }
        }

        private void DisplayMessage()
        {
            if (_messageQueue.Count <= 0)
            {
                _isBusy = false;
                body.SetActive(false);
                return;
            }

            _isBusy = true;
            
            KeyValuePair<string, EMessageType> message = _messageQueue.Dequeue();
            textMesh.text = message.Key;
            textMesh.color = _colorMap[message.Value];
            
            animator.SetTrigger(Helpers.Strings.Show);
            
            body.SetActive(true);
        }
    }
}
