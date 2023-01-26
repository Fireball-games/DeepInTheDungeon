using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using TMPro;
using UnityEngine;

public class MessageBar : MonoBehaviour
{
    [SerializeField] private float fadingTime = 1.5f;
    [SerializeField] private float delayBeforeNextMessage = 0.5f;
    [SerializeField] private float minimumShowTime = 1f;

    public string Text => _message.text;

    private TextMeshProUGUI _message;
    private Queue<MessageItem> _messages;
    private Coroutine _fadeCoroutine;
    private Coroutine _automaticDismissCoroutine;
    private Dictionary<EMessageType, Color> _colorsByType;
    private MessageItem _currentMessage;
    private WaitForSecondsRealtime _delayBeforeNextMessage;
    private bool _isClosing;
    private float _currentMessageShowTime;
    private float _automaticDismissStartedTime;

    public enum EMessageType
    {
        Normal = 0,
        Positive = 1,
        Negative = 2,
        Warning = 3,
    }

    private void Awake()
    {
        _message = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        _messages = new Queue<MessageItem>();
        _delayBeforeNextMessage = new WaitForSecondsRealtime(delayBeforeNextMessage);

        _colorsByType = new Dictionary<EMessageType, Color>
        {
            {EMessageType.Normal, Colors.Black},
            {EMessageType.Positive, Colors.Positive},
            {EMessageType.Negative, Colors.Negative},
            {EMessageType.Warning, Colors.Warning},
        };

        DeactivateMessage();
    }

    private void OnDisable()
    {
        Close(true);
    }

    /// <summary>
    /// Sets the message.
    /// </summary>
    /// <param name="messageText">Text of the message.</param>
    /// <param name="messageType">Normal | Negative | Positive</param>
    /// <param name="fadeOnCancel">If message should fade upon cancel. Default is true.</param>
    /// <param name="automaticDismissDelay">If message is not canceled, how long should stay before canceling itself. Default is forever (null).</param>
    public void Set(string messageText, EMessageType messageType = EMessageType.Normal, bool fadeOnCancel = true,
        float? automaticDismissDelay = null)
    {
        // This prevents further actions when Set is called before Awake, which is either desired or meaningful.
        if (_messages == null) return;

        if (string.IsNullOrEmpty(messageText))
        {
            Close();
            return;
        }

        _isClosing = false;
        _messages.Enqueue(new MessageItem
        {
            Text = messageText,
            FadeOnCancel = fadeOnCancel,
            MessageType = messageType,
            AutomaticDismissDelay = automaticDismissDelay
        });

        if (_currentMessage == null)
        {
            DequeueMessage();
        }
        else
        {
            FadeCurrentMessage();
        }
    }

    /// <summary>
    /// Dismisses the message and all messages in a queue.
    /// </summary>
    /// <param name="closeImmediately">Close immediately even if message is set to fade upon cancel. Default is false.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Close(bool closeImmediately = false)
    {
        if (_currentMessage == null || _isClosing) return;

        _isClosing = true;
        _messages.Clear();

        if (!closeImmediately)
        {
            FadeCurrentMessage(false);
        }
        else
        {
            DeactivateMessage();
        }
    }

    private void FadeCurrentMessage(bool dequeAfterFade = true) =>
        _fadeCoroutine ??= StartCoroutine(FadeMessageCo(dequeAfterFade));

    private IEnumerator FadeMessageCo(bool dequeueAfterFade = true)
    {
        while (Time.time - _currentMessageShowTime < minimumShowTime)
        {
            yield return null;
        }

        if (_currentMessage is {FadeOnCancel: true})
        {
            while (_message.color.a > 0)
            {
                SetMessageTextAlpha(_message.color.a - 1 / (fadingTime / Time.deltaTime));
                yield return null;
            }

            if (!dequeueAfterFade)
            {
                DeactivateMessage();
            }

            if (_messages.Any())
            {
                yield return _delayBeforeNextMessage;
                DequeueMessage();
                yield break;
            }

            _fadeCoroutine = null;
        }
        else
        {
            DequeueMessage();
        }
    }

    private IEnumerator AutomaticDismissCoroutine(float targetMessageStartTime)
    {
        while (!_isClosing && Time.time - _automaticDismissStartedTime < _currentMessage.AutomaticDismissDelay)
        {
            yield return null;
        }

        if (_currentMessage != null
            && Math.Abs(_currentMessage.DismissStartTime - targetMessageStartTime) < Single.Epsilon)
        {
            FadeCurrentMessage();
        }
    }

    private void DequeueMessage()
    {
        StopAutomaticDismissCoroutine();

        if (!_messages.Any() || _isClosing)
        {
            DeactivateMessage();
            return;
        }

        _fadeCoroutine = null;
        _currentMessage = _messages.Dequeue();
        SetMessageTextAlpha(1);


        _message.gameObject.SetActive(true);


        _message.text = _currentMessage.Text;
        _message.color = _colorsByType[_currentMessage.MessageType];
        _currentMessageShowTime = Time.time;

        if (_currentMessage.AutomaticDismissDelay != null)
        {
            _automaticDismissStartedTime = Time.time;
            _currentMessage.DismissStartTime = _automaticDismissStartedTime;
            _automaticDismissCoroutine = StartCoroutine(AutomaticDismissCoroutine(_automaticDismissStartedTime));
        }

        if (_messages.Any())
        {
            FadeCurrentMessage();
        }
    }

    private void SetMessageTextAlpha(float value)
    {
        Color messageColor = _message.color;
        messageColor.a = value;
        _message.color = messageColor;
    }

    private void DeactivateMessage()
    {
        SetMessageTextAlpha(1);
        _message.text = "";
        _currentMessage = null;
        _messages.Clear();

        StopAutomaticDismissCoroutine();

        _message.gameObject.SetActive(false);
    }

    private void StopAutomaticDismissCoroutine()
    {
        if (_automaticDismissCoroutine == null) return;

        StopCoroutine(AutomaticDismissCoroutine(float.MaxValue));
        _automaticDismissCoroutine = null;
    }

    private class MessageItem
    {
        public string Text;
        public bool FadeOnCancel;
        public float? AutomaticDismissDelay;
        public EMessageType MessageType;
        public float DismissStartTime;
    }
}