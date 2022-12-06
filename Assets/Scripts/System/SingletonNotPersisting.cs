using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System
{
    public class SingletonNotPersisting<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance;
        
        protected virtual void Awake ()
        {
            if ( Instance == null )
            {
                Instance = this as T;
            }
            else
            {
                Logger.LogWarning($"Second instance of {gameObject.name} tries to instantiate.");
                GameObject o;
                (o = gameObject).SetActive(false);
                Destroy ( o );
            }
        }
    }
}