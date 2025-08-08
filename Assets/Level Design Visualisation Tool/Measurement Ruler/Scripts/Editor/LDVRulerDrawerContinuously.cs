using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LDVRulerDrawerContinuously
{
    public static bool continuouslyDrawingMeasurementLines = false;
    
    static LDVRulerDrawerContinuously()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!Application.isPlaying) return;
        
        LDVManager ldvManagerReference = Object.FindFirstObjectByType<LDVManager>();
        if (!ldvManagerReference) return;

        DrawMeasurementLines(ldvManagerReference);
    }
    
    private static Texture2D FakingBorderWithTexture(int colourWidth, int colourHeight, Color aColourThatIsStored)
    {
        Color[] storingAPixelColourInAnArray = new Color[colourWidth * colourHeight];
        for (int i = 0; i < storingAPixelColourInAnArray.Length; i++)
            storingAPixelColourInAnArray[i] = aColourThatIsStored;
    
        Texture2D result = new Texture2D(colourWidth, colourHeight);
        result.SetPixels(storingAPixelColourInAnArray);
        result.Apply();
        return result;
    }

    private static void DrawMeasurementLines(LDVManager ldvManagerReference)
    {
        if (ldvManagerReference.LDVRulerSpheresList.Count >= 2)
        {
            if (continuouslyDrawingMeasurementLines)
            {
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
            
                    // fixing the measurement text so that it is shown in the middle with text more easier to look at
                    GUIStyle alignmentForMeasurement = new GUIStyle();
                    alignmentForMeasurement.alignment = TextAnchor.MiddleCenter;
                    alignmentForMeasurement.fontStyle = FontStyle.Bold;
                    alignmentForMeasurement.normal.textColor = Color.white;
            
                    // Background styling
                    alignmentForMeasurement.normal.background = FakingBorderWithTexture(1, 1, new Color(0, 0, 0, 1));
                    int borderPadding = 1;
                    alignmentForMeasurement.padding = new RectOffset(borderPadding, borderPadding, borderPadding, borderPadding);
            
                    // displays the distance between the lines, and limits it to 2 decimal points
                    Handles.Label(midpoint, $"Distance: {distanceBetweenEachMeasurementSphere:F2} units", alignmentForMeasurement);
                }
            }
        }
    }
}
