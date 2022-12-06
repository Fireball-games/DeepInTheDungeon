using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Fireball Games * * * PetrZavodny.com

namespace Scripts.UI.Components
{
    public class StatusBar : MonoBehaviour
    {
        [SerializeField] private Color normalColor;
        [SerializeField] private Color positiveColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color negativeColor;
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject body;

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
                {EMessageType.None, normalColor},
                {EMessageType.Positive, positiveColor},
                {EMessageType.Warning, warningColor},
                {EMessageType.Negative, negativeColor},
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
