using UnityEngine;

namespace Scripts.System
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
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
                Destroy ( o );
            }
        }

        #endregion
	
    }
}