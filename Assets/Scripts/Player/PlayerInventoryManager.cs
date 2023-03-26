using System;
using Scripts.InventoryManagement.Inventories;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        public Inventory Inventory { get; private set; }
        public ActionStore ActionStore { get; private set; }
        public Equipment Equipment { get; private set; }
        
        private void Awake()
        {
            Inventory = GetComponent<Inventory>();
            ActionStore = GetComponent<ActionStore>();
            Equipment = GetComponent<Equipment>();
        }
    }
}