using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserCubeBeams : MonoBehaviour
{
    private List<LaserBeamController> _beamControllers = new();
    private void Awake()
    {
        _beamControllers = GetComponentsInChildren<LaserBeamController>().ToList();
        
        foreach (var beam in _beamControllers)
        {
            beam.ActivateBeam(); 
        }
    }
    
    
}
