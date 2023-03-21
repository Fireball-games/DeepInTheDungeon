using NaughtyAttributes;
using Scripts.Helpers.Attributes;
using UnityEngine;

namespace Scripts.Helpers
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private bool rotateX;
        [SerializeField] private bool rotateY;
        [SerializeField] private bool rotateZ;
        [SerializeField, HideIf(nameof(useUniformSpeed)), ShowWhen(nameof(rotateX))] private float xSpeed;
        [SerializeField, HideIf(nameof(useUniformSpeed)), ShowWhen(nameof(rotateY))] private float ySpeed;
        [SerializeField, HideIf(nameof(useUniformSpeed)), ShowWhen(nameof(rotateZ))] private float zSpeed;
        [SerializeField] private bool useUniformSpeed;
        [SerializeField, ShowIf(nameof(useUniformSpeed))] private float uniformSpeed;

        public bool isActive = true;

        private void Update()
        {
            if (!isActive) return;

            float currentXSpeed = xSpeed;
            float currentYSpeed = ySpeed;
            float currentZSpeed = zSpeed;

            if (useUniformSpeed)
            {
                currentXSpeed = currentYSpeed = currentZSpeed = uniformSpeed;
            }

            if (rotateX)
            {
                transform.Rotate(Vector3.right, currentXSpeed * Time.deltaTime);
            }
            if (rotateY)
            {
                transform.Rotate(Vector3.up, currentYSpeed * Time.deltaTime);
            }
            if (rotateZ)
            {
                transform.Rotate(Vector3.forward, currentZSpeed * Time.deltaTime);
            }
        }
    }
}