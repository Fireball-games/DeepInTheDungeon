using UnityEngine;

namespace Scripts.System.MonoBases
{
    /// <summary>
    /// Prevention of accessing singleton on application quit.
    /// </summary>
    public abstract class SingletonBase : MonoBehaviour
    {
        protected static bool IsApplicationExiting;

        private void OnApplicationQuit()
        {
            IsApplicationExiting = true;
        }
    }
    
    public abstract class Singleton<T> : SingletonBase where T : Component
    {
	
        #region Fields

        /// <summary>
        /// The instance.
        /// </summary>
        private static T instance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (IsApplicationExiting) return null;
                
                if (instance) return instance;
                
                instance = FindObjectOfType<T> ();
                    
                if (instance) return instance;
                    
                GameObject obj = new()
                {
                    name = typeof ( T ).Name
                };

                instance = obj.AddComponent<T> ();
                
                return instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake ()
        {
            if ( instance == null )
            {
                instance = this as T;
                DontDestroyOnLoad ( gameObject );
            }
            else
            {
                GameObject o;
                (o = gameObject).SetActive(false);
                Destroy(o);
            }
        }

        #endregion
	
    }
}