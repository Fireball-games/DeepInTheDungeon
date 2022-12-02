using System;
using System.Collections.Generic;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts
{
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController player;

        public static List<List<int>> CurrentMapLayout => _currentMap.Layout;
        public static bool MovementEnabled => _movementEnabled;

        private static MapDescription _currentMap;
        private static bool _movementEnabled;

        protected override void Awake()
        {
            base.Awake();
            
            mapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnEnable()
        {
            EventsManager.OnStartGameRequested += OnStartGameRequested;
        }

        private void OnDisable()
        {
            EventsManager.OnStartGameRequested -= OnStartGameRequested;
        }

        private void StartLevel(MapDescription map)
        {
            _movementEnabled = false;
            _currentMap = map;
            mapBuilder.BuildMap(_currentMap);
            
        }

        private void OnStartGameRequested()
        {
            StartLevel(new MapDescription());
        }

        private void OnLayoutBuilt()
        {
            player = Instantiate(playerPrefab);
            player.SetPosition(_currentMap.StartPosition.ToVector3());
            _movementEnabled = true;
            EventsManager.TriggerOnLevelStarted();
        }
    }
}