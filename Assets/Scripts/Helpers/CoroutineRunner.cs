using System.Collections;

namespace Scripts.Helpers
{
    using UnityEngine;

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static Coroutine Run(IEnumerator coroutine)
        {
            return _instance.StartCoroutine(coroutine);
        }

        public static void Stop(IEnumerator coroutine)
        {
            _instance.StopCoroutine(coroutine);
        }
    }
}