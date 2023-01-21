using Scripts.EventsManagement;
using Scripts.System;
using TMPro;
using UnityEngine;

namespace Scripts.UI.PlayMode
{
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

      private void OnLevelStarted() => OnPlayerPositionChanged(GameManager.Instance.Player.transform.position);
   }
}
