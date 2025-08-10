using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LDVRulerManager : MonoBehaviour
{
    public static LDVRulerManager ldvRulerManagerInstance {get; private set;}
    
    [Header("GameObjects that are to have a measurement")]
    public List<GameObject> LDVRulerSpheresList = new();
    
    private void Awake()
    {
        if (ldvRulerManagerInstance && ldvRulerManagerInstance != this)
        {
            Destroy(this);
            return;
        }
        ldvRulerManagerInstance = this;
    }
    
    public void CheckForMeasurementSpheresThenAddToList(GameObject measurementSpheres)
    {
        if (!LDVRulerSpheresList.Contains(measurementSpheres))
        {
            LDVRulerSpheresList.Add(measurementSpheres);
            Debug.Log($"Added ruler sphere: {measurementSpheres.name}");
        }
    }
}
