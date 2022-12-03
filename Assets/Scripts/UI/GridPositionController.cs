using Scripts;
using TMPro;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

public class GridPositionController : MonoBehaviour
{
   [SerializeField] private TMP_Text text;

   private void OnEnable()
   {
      EventsManager.OnPlayerPositionChanged += OnPlayerPositionChanged;
      EventsManager.OnLevelStarted += OnLevelStarted;
   }

   private void OnDisable()
   {
      EventsManager.OnPlayerPositionChanged += OnPlayerPositionChanged;
      EventsManager.OnLevelStarted -= OnLevelStarted;
   }

   private void OnPlayerPositionChanged(Vector3 newPosition)
   {
      text.text = $"{Mathf.RoundToInt(newPosition.x)} : {Mathf.RoundToInt(newPosition.z)}";
   }

   private void OnLevelStarted() => OnPlayerPositionChanged(GameController.Instance.Player.transform.position);
}