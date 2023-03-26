using System;
using Scripts.Inventory.Inventories;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        public Inventory.Inventories.Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        
        private void Awake()
        {
            Inventory = GetComponent<Inventory.Inventories.Inventory>();
            ActionStore = GetComponent<ActionStore>();
            Equipment = GetComponent<Equipment>();
        }
    }
}