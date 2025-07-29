using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LDVManager))]
public class LDVManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        LDVManager ldvManagerReference = (LDVManager)target;

        if (ldvManagerReference.LDVRulerSpheresList.Count < 2)
        {
            return;
        }

        for (int i = 0; i < ldvManagerReference.LDVRulerSpheresList.Count - 1; i++)
        {
            // line colour
            Handles.color = Color.cyan;
            
            // makes vector3 positions from the game objects so the API can read them
            Vector3 measurementSpherePosition1 = ldvManagerReference.LDVRulerSpheresList[i].transform.position;
            Vector3 measurementSpherePosition2 = ldvManagerReference.LDVRulerSpheresList[i + 1].transform.position;

            // Draws the dotted lines, and displays the distances between them measuring spheres
            Handles.DrawDottedLine(measurementSpherePosition1, measurementSpherePosition2, 4f);
            float distanceBetweenEachMeasurementSphere = Vector3.Distance(measurementSpherePosition1, measurementSpherePosition2);
            Vector3 midpoint = (measurementSpherePosition1 + measurementSpherePosition2) / 2f;

            // displays the distance between the lines, and limits it to 2 decimal points
            Handles.Label(midpoint, $"Distance: {distanceBetweenEachMeasurementSphere:F2} units");
        }
    }
}
