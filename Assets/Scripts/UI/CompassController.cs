using System.Collections;
using Scripts;
using UnityEngine;

public class CompassController : MonoBehaviour
{
   [SerializeField] private GameObject compassImage;
   
   private void OnEnable()
   {
      EventsManager.OnPlayerRotationChanged += Rotate;
      EventsManager.OnLevelStarted += SetRotation;
   }

   private void OnDisable()
   {
      EventsManager.OnPlayerRotationChanged -= Rotate;
      EventsManager.OnLevelStarted -= SetRotation;
   }
   
   private void Rotate(Vector3 targetRotation) => StartCoroutine(RotateCoroutine(targetRotation));

   private IEnumerator RotateCoroutine(Vector3 targetRotation)
   {
      targetRotation.y += 90;
      
      if (targetRotation.y is > 270f and < 361f) targetRotation.y = 0f;
      if (targetRotation.y < 0f) targetRotation.y = 270f;
      
      targetRotation.z = targetRotation.y;
      targetRotation.y = 0;

      while (Vector3.Distance(compassImage.transform.eulerAngles, targetRotation) > 0.05)
      {
         compassImage.transform.rotation = Quaternion.RotateTowards(compassImage.transform.rotation,
            Quaternion.Euler(targetRotation),
            Time.deltaTime * PlayerController.TransitionRotationSpeed );
         
         yield return null;
      }
      
      compassImage.transform.rotation = Quaternion.Euler(targetRotation);
   }

   private void SetRotation()
   {
      Vector3 playerRotation = GameController.Instance.Player.transform.rotation.eulerAngles;
      compassImage.transform.rotation = Quaternion.Euler(new(0f, 0f, playerRotation.y + 90));
   }

}
