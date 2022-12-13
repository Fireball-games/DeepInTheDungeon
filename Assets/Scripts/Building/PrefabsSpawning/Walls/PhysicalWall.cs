using System.Collections;
using System.Collections.Generic;
using Scripts.Building.Walls;
using UnityEngine;

public class PhysicalWall : MonoBehaviour, IPhysicalPrefab
{
    public float offset;
    public string prefabName { get; set; }
    public Vector3 position { get; set; }
    public Vector3 prefabPosition { get; set; }
}
