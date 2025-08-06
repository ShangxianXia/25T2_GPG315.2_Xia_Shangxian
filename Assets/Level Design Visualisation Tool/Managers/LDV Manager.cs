using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LDVManager : MonoBehaviour
{
    public static LDVManager LdvManagerInstance {get; private set;}
    
    [Header("GameObjects that are to have a measurement")]
    public List<GameObject> LDVRulerSpheresList = new();
    
    private void Awake()
    {
        if (LdvManagerInstance && LdvManagerInstance != this)
        {
            Destroy(this);
            return;
        }
        LdvManagerInstance = this;
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
