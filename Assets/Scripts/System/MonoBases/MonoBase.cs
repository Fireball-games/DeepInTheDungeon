using UnityEngine;

namespace Scripts.System.MonoBases
{
    public class MonoBase : MonoBehaviour
    {
        private Transform _transform;
        public new Transform transform
        {
            get
            {
                _transform ??= base.transform;
                return _transform;
            }
        }
        
        private Vector3 _position;
        public Vector3 position
        {
            get
            {
                _position = transform.position;
                return _position;
            }
            set => transform.position = value;
        }

        private GameObject _body;
        public GameObject body
        {
            get
            {
                _body ??= transform.Find("Body").gameObject;
                
                return _body;
            }
        }
        
        public virtual void SetActive(bool value) => body.SetActive(value);
    }
}