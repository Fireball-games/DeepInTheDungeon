using UnityEngine;

namespace Scripts.System.MonoBases
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
                GameObject o;
                (o = gameObject).SetActive(false);
                Destroy ( o );
            }
        }
    }
}