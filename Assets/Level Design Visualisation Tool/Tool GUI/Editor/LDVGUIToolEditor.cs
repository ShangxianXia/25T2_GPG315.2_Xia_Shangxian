using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Codice.CM.SEIDInfo;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public class LDVGUIToolEditor : EditorWindow
{
    private enum WindowTabs {MainLDVMenu, LDVRuler, LDVGameObjectElements, LDVHeatMapConfigurations}
    private WindowTabs currentWindowTab = WindowTabs.MainLDVMenu;

    [MenuItem("LDV/Level Design Visualisation Tool")]
    public static void ShowLDVToolWindow()
    {
        GetWindow<LDVGUIToolEditor>("Level Design Visualisation Tool");
    }

    void OnGUI()
    {
        currentWindowTab = (WindowTabs)GUILayout.Toolbar((int)currentWindowTab, System.Enum.GetNames(typeof(WindowTabs)));

        switch (currentWindowTab)
        {
            case WindowTabs.MainLDVMenu:
                MainLDVMenu();
                break;
            
            case WindowTabs.LDVRuler:
                LDVRulerStuff();
                break;
            
            case WindowTabs.LDVGameObjectElements:
                LDVGameObjectModifiableElements();
                break;
            
            case WindowTabs.LDVHeatMapConfigurations:
                LDVHeatMapConfigurations();
                break;
        }
    }

    void MainLDVMenu()
    {
         // custom style
        GUIStyle myOwnStyleForAHeader = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(10, 10, 10, 10)
        };
        
        GUILayout.Label("This is the LDV Tool,\n to begin select the tabs up top that correspond to the feature\n Or press the buttons on the bottom.", myOwnStyleForAHeader);

        if (GUILayout.Button("Measuring Ruler?", GUILayout.Height(30f)))
        {
            currentWindowTab = WindowTabs.LDVRuler;
        }

        if (GUILayout.Button("Object Elements?", GUILayout.Height(30f)))
        {
            currentWindowTab = WindowTabs.LDVGameObjectElements;
        }
        
        if (GUILayout.Button("Heat map configurations?", GUILayout.Height(30f)))
        {
            currentWindowTab = WindowTabs.LDVHeatMapConfigurations;
        }
    }
    
    
    private static GameObject rulerSpherePrefab;
    void LDVRulerStuff()
    {
        EditorGUILayout.HelpBox("The continouously line drawer only works in play mode!", MessageType.Warning);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select/De-Select LDV Manager"))
        {
            bool LDVManagerSelected = false;
            foreach (GameObject gameObjectsThatAreBeingCheckedToSeeIfTheyHaveLDVManagerReference in Selection.gameObjects)
            {
                if (gameObjectsThatAreBeingCheckedToSeeIfTheyHaveLDVManagerReference.gameObject.GetComponent<LDVManager>())
                {
                    LDVManagerSelected = true;
                    break;
                }
            }
            
            if (LDVManagerSelected)
            {
                Selection.activeObject = null;
            }
            else
            {
                LDVManager[] LDVManagerReferenceList = FindObjectsByType<LDVManager>(FindObjectsSortMode.None);
                if (LDVManagerReferenceList.Length > 0)
                {
                    LDVManagerSelected = true;
                    Selection.activeObject = LDVManagerReferenceList[0].gameObject;
                    EditorGUIUtility.PingObject(LDVManagerReferenceList[0].gameObject);

                    if (LDVManagerReferenceList.Length > 1)
                    {
                        Debug.Log("Theres multiple LDV Managers found, only first is selected.");
                    }
                }
                else
                {
                    Debug.Log("There are no LDV Managers found.");
                }
            }
        }

        if (GUILayout.Button("Turn On/Off continuous drawn distances"))
        {
            bool showLDVRuler = LDVRulerDrawerContinuously.continuouslyDrawingMeasurementLines;
            if (showLDVRuler)
            {
                LDVRulerDrawerContinuously.continuouslyDrawingMeasurementLines = false;
                showLDVRuler = false;
            }
            else
            {
                LDVRulerDrawerContinuously.continuouslyDrawingMeasurementLines = true;
                showLDVRuler = true;
            }
        }
        
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Add a measurement sphere"))
        {
            string[] pathToTheMeasurementSpherePrefab = AssetDatabase.FindAssets("MeasurementSpheres t:prefab");
        
            if (pathToTheMeasurementSpherePrefab.Length == 0)
            {
                Debug.LogError("RulerSpherePrefab not found in the project!");
            }
            else if (pathToTheMeasurementSpherePrefab.Length >= 1)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(pathToTheMeasurementSpherePrefab[0]);
                rulerSpherePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (rulerSpherePrefab)
                {
                    Debug.Log("A path found to the measurement spheres prefab.");
                    GameObject newMeasurementSphere = Instantiate(rulerSpherePrefab);
                    LDVManager.LdvManagerInstance.CheckForMeasurementSpheresThenAddToList(newMeasurementSphere);
                }
                else
                {
                    Debug.LogError("Failed to load RulerSpherePrefab!");
                }
            }
        }
    }
    
    private float scaleValue = 1;
    private float rotationValue = 360;
    private GameObject prefabForObjectInstantiation;
    // Advanced position values
    private float advancedXPositionValue = 0;
    private float advancedYPositionValue = 0;
    private float advancedZPositionValue = 0;
    // Advanced rotation values
    private float advancedXRotationValue = 0;
    private float advancedYRotationValue = 0;
    private float advancedZRotationValue = 0;
    // Advanced scale values
    private float advancedXScaleValue = 0;
    private float advancedYScaleValue = 0;
    private float advancedZScaleValue = 0;
    void LDVGameObjectModifiableElements()
    {
        // Object transform / Colour modifications //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        GUILayout.Space(10f);
        GUILayout.Label("Reset Selected Game Objects", EditorStyles.boldLabel);
    
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset all"))
        {
            Undo.RecordObjects(Selection.transforms, "Reset all of selected Game Objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer) renderer.material.color = Color.white;
                obj.transform.localScale = Vector3.one;
            }
        }

        if (GUILayout.Button("Reset position"))
        {
            Undo.RecordObjects(Selection.transforms, "Reset positions of selected Game Objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.position = Vector3.zero;
            }
        }
        
        if (GUILayout.Button("Reset rotation"))
        {
            Undo.RecordObjects(Selection.transforms, "Reset rotations of selected Game Objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.rotation = Quaternion.identity;
            }
        }
    
        if (GUILayout.Button("Reset scale"))
        {
            Undo.RecordObjects(Selection.transforms, "Reset scales of selected Game Objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.localScale = Vector3.one;
            }
        }

        if (GUILayout.Button("Reset colour"))
        {
            Undo.RecordObjects(Selection.gameObjects.SelectMany(o => o.GetComponents<MeshRenderer>()).ToArray(), "Reset colours of selected Game Objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer) renderer.material.color = Color.white;
            }
        }
        GUILayout.EndHorizontal();
    
    
        GUILayout.Space(10);
    
        // Rotation randomisation ///////////////////////////////////////////////
        GUILayout.Label($"Randomise Selected Game Objects Rotations by > {rotationValue} < degrees", EditorStyles.boldLabel);
        rotationValue = GUILayout.HorizontalSlider(rotationValue, 0, 360f);
        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("X Axis"))        
        {
            Undo.RecordObjects(Selection.transforms, "Randomised X Rotations");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.Rotate(Vector3.right, Random.Range(0f, rotationValue));
            }
        }
    
        if (GUILayout.Button("Y Axis"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised Y Rotations");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.Rotate(Vector3.up, Random.Range(0f, rotationValue));
            }
        }
    
        if (GUILayout.Button("Z Axis"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised Z Rotations");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.Rotate(Vector3.forward, Random.Range(0f, rotationValue));
            }
        }
    
        if (GUILayout.Button("All Axis"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised All Rotations");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.Rotate(new Vector3(Random.Range(0f, rotationValue), Random.Range(0f, rotationValue), Random.Range(0f, rotationValue)));
            }
        }
        GUILayout.EndHorizontal();
    
        GUILayout.Space(10);
    
        // scale randomisation ///////////////////////////////////////////////
        GUILayout.Label($"Randomise Selected Game Objects Scales by > {scaleValue} <", EditorStyles.boldLabel);
        scaleValue = GUILayout.HorizontalSlider(scaleValue, 1, 100);
        GUILayout.Space(15f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("X Scale"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised X Scales of selected game objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                Vector3 currentScale = obj.transform.localScale;
                obj.transform.localScale = new Vector3(Random.Range(0, scaleValue), currentScale.y, currentScale.z);
            }
        }
    
        if (GUILayout.Button("Y Scale"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised Y Scales of selected game objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                Vector3 currentScale = obj.transform.localScale;
                obj.transform.localScale = new Vector3(currentScale.x, Random.Range(0, scaleValue), currentScale.z);
            }
        }
    
        if (GUILayout.Button("Z Scale"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised Z Scales of selected game objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                Vector3 currentScale = obj.transform.localScale;
                obj.transform.localScale = new Vector3(currentScale.x, currentScale.y, Random.Range(0, scaleValue));
            }
        }
    
        if (GUILayout.Button("All Size/Scale"))
        {
            Undo.RecordObjects(Selection.transforms, "Randomised All Scales of selected game objects");
            foreach (GameObject obj in Selection.gameObjects)
            {
                obj.transform.localScale = new Vector3(Random.Range(0, scaleValue), Random.Range(0, scaleValue), Random.Range(0, scaleValue));
            }
        }
        GUILayout.EndHorizontal();
    
        GUILayout.Space(10);
    
        // colour randomisation ///////////////////////////////////////////////
        GUILayout.Label("Colour Randomisation for Selected Game Objects", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomise All Colour"))
        {
            var renderers = Selection.gameObjects.SelectMany(o => o.GetComponents<MeshRenderer>()).ToArray();
        
            Undo.RecordObjects(renderers, "Randomized all Colours of selected game objects");
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material.color = new Color(Random.Range(0f, 5f),Random.Range(0f, 5f),Random.Range(0f, 5f));
            }
        }
        
        if (GUILayout.Button("Randomise Red Colour"))
        {
            var renderers = Selection.gameObjects.SelectMany(o => o.GetComponents<MeshRenderer>()).ToArray();
        
            Undo.RecordObjects(renderers, "Randomized red Colour of selected game objects");
            foreach (MeshRenderer renderer in renderers)
            {
                Color original = renderer.material.color;
                renderer.material.color = new Color(Random.Range(0f, 5f), original.g ,original.b);
            }
        }   
        
        if (GUILayout.Button("Randomise Green Colour"))
        {
            var renderers = Selection.gameObjects.SelectMany(o => o.GetComponents<MeshRenderer>()).ToArray();
        
            Undo.RecordObjects(renderers, "Randomized green Colour of selected game objects");
            foreach (MeshRenderer renderer in renderers)
            {
                Color original = renderer.material.color;
                renderer.material.color = new Color(original.r, Random.Range(0f, 5f) ,original.b);
            }
        }
        
        if (GUILayout.Button("Randomise Blue Colour"))
        {
            var renderers = Selection.gameObjects.SelectMany(o => o.GetComponents<MeshRenderer>()).ToArray();
        
            Undo.RecordObjects(renderers, "Randomized blue Colour of selected game objects");
            foreach (MeshRenderer renderer in renderers)
            {
                Color original = renderer.material.color;
                renderer.material.color = new Color(original.r, original.g ,Random.Range(0f, 5f));
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Object Instantiation //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Advanced Coordination", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset all advanced coordinates", GUILayout.ExpandWidth(false)))
            {
                advancedXPositionValue = 0;
                advancedYPositionValue = 0;
                advancedZPositionValue = 0;
                
                advancedXRotationValue = 0;
                advancedYRotationValue = 0;
                advancedZRotationValue = 0;
                
                advancedXScaleValue = 0;
                advancedYScaleValue = 0;
                advancedZScaleValue = 0;
            }
            if (GUILayout.Button("Reset advanced position", GUILayout.ExpandWidth(false)))
            {
                advancedXPositionValue = 0;
                advancedYPositionValue = 0;
                advancedZPositionValue = 0;
            }
            if (GUILayout.Button("Reset advanced rotation", GUILayout.ExpandWidth(false)))
            {
                advancedXRotationValue = 0;
                advancedYRotationValue = 0;
                advancedZRotationValue = 0;
            }
            if (GUILayout.Button("Reset advanced scale", GUILayout.ExpandWidth(false)))
            {
                advancedXScaleValue = 0;
                advancedYScaleValue = 0;
                advancedZScaleValue = 0;
            }
        }
        GUILayout.EndHorizontal();

        // Displays the current values of vector positions
        GUILayout.BeginHorizontal();
        {
            // Advanced Vector position X ///////////////////////////////////////////////
            GUILayout.Label($"Current X Position: {advancedXPositionValue:F2}");
            advancedXPositionValue = GUILayout.HorizontalSlider(advancedXPositionValue, -500, 500);
            string inputTextXPosition = GUILayout.TextField(advancedXPositionValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextXPosition, out float newXPositionValue))
            {
                advancedXPositionValue = newXPositionValue;
            }
            else
            {
                advancedXPositionValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Vector position Y ///////////////////////////////////////////////
            GUILayout.Label($"Current Y Position: {advancedYPositionValue:F2}");
            advancedYPositionValue = GUILayout.HorizontalSlider(advancedYPositionValue, -500, 500);
            string inputTextYPosition = GUILayout.TextField(advancedYPositionValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextYPosition, out float newYPositionValue))
            {
                advancedYPositionValue = newYPositionValue;
            }
            else
            {
                advancedYPositionValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Vector position Z ///////////////////////////////////////////////
            GUILayout.Label($"Current Z Position: {advancedZPositionValue:F2}");
            advancedZPositionValue = GUILayout.HorizontalSlider(advancedZPositionValue, -500, 500);
            string inputTextZPosition = GUILayout.TextField(advancedZPositionValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextZPosition, out float newZPositionValue))
            {
                advancedZPositionValue = newZPositionValue;
            }
            else
            {
                advancedZPositionValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced X Rotation ////////////////////////////////////////////////
            GUILayout.Label($"Current X Rotation: {advancedXRotationValue:F2}");
            advancedXRotationValue = GUILayout.HorizontalSlider(advancedXRotationValue, -360, 360);
            string inputTextXRotation = GUILayout.TextField(advancedXRotationValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextXRotation, out float newXRotationValue))
            {
                advancedXRotationValue = newXRotationValue;
            }
            else
            {
                advancedXRotationValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Y Rotation ///////////////////////////////////////////////
            GUILayout.Label($"Current Y Rotation: {advancedYRotationValue:F2}");
            advancedYRotationValue = GUILayout.HorizontalSlider(advancedYRotationValue, -360, 360);
            string inputTextYRotation = GUILayout.TextField(advancedYRotationValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextYRotation, out float newYRotationValue))
            {
                advancedYRotationValue = newYRotationValue;
            }
            else
            {
                advancedYRotationValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Z Rotation ///////////////////////////////////////////////
            GUILayout.Label($"Current Z Rotation: {advancedZRotationValue:F2}");
            advancedZRotationValue = GUILayout.HorizontalSlider(advancedZRotationValue, -360, 360);
            string inputTextZRotation = GUILayout.TextField(advancedZRotationValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextZRotation, out float newZRotationValue))
            {
                advancedZRotationValue = newZRotationValue;
            }
            else
            {
                advancedZRotationValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced X Scale ///////////////////////////////////////////////
            GUILayout.Label($"Current X Scale: {advancedXScaleValue:F2}");
            advancedXScaleValue = GUILayout.HorizontalSlider(advancedXScaleValue, 0, 100);
            string inputTextXScale = GUILayout.TextField(advancedXScaleValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextXScale, out float newXScaleValue))
            {
                advancedXScaleValue = newXScaleValue;
            }
            else
            {
                advancedXScaleValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Y Scale ///////////////////////////////////////////////
            GUILayout.Label($"Current Y Scale: {advancedYScaleValue:F2}");
            advancedYScaleValue = GUILayout.HorizontalSlider(advancedYScaleValue, 0, 100);
            string inputTextYScale = GUILayout.TextField(advancedYScaleValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextYScale, out float newYScaleValue))
            {
                advancedYScaleValue = newYScaleValue;
            }
            else
            {
                advancedYScaleValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            // Advanced Z Scale ///////////////////////////////////////////////
            GUILayout.Label($"Current Z Scale: {advancedZScaleValue:F2}");
            advancedZScaleValue = GUILayout.HorizontalSlider(advancedZScaleValue, 0, 100);
            string inputTextZScale = GUILayout.TextField(advancedZScaleValue.ToString("F2"), EditorStyles.textField, GUILayout.ExpandWidth(false));
            if (float.TryParse(inputTextZScale, out float newZScaleValue))
            {
                advancedZScaleValue = newZScaleValue;
            }
            else
            {
                advancedZScaleValue = 0;
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        {
            // apply above coordinate values to the selected game object
            if (GUILayout.Button("Apply Coordinates above to selected game object?", GUILayout.ExpandWidth(false)))
            {
                if (Selection.activeGameObject)
                {
                    Vector3 newCoordinatesForSelectedGameObject = new Vector3(advancedXPositionValue, advancedYPositionValue, advancedZPositionValue);
                
                    Undo.RecordObject(Selection.activeGameObject.transform, $"New Coordinates for selected game object: {Selection.activeGameObject.gameObject.name}");
                    Selection.activeGameObject.transform.position = newCoordinatesForSelectedGameObject;
                }
                else
                {
                    Debug.LogWarning("No selected game object to apply coordinate values to.");
                }
            }
            
            // copies and pastes the vectors of the selected game object to the values above
            if (GUILayout.Button("Copy and Paste Coordinates of selected game object to values above?", GUILayout.ExpandWidth(false)))
            {
                if (Selection.activeGameObject)
                {
                    // copies the position values of selected game object
                    Vector3 newCoordinates = Selection.activeGameObject.transform.position;
                    advancedXPositionValue = newCoordinates.x;
                    advancedYPositionValue = newCoordinates.y;
                    advancedZPositionValue = newCoordinates.z;
                    
                    // copies the rotation values of selected game object
                    Vector3 newEulerRotationAngles = Selection.activeGameObject.transform.rotation.eulerAngles;
                    advancedXRotationValue = newEulerRotationAngles.x;
                    advancedYRotationValue = newEulerRotationAngles.y;
                    advancedZRotationValue = newEulerRotationAngles.z;
                    
                    // copies the scale values of selected game object
                    Vector3 newScale = Selection.activeGameObject.transform.localScale;
                    advancedXScaleValue = newScale.x;
                    advancedYScaleValue = newScale.y;
                    advancedZScaleValue = newScale.z;
                }
                else
                {
                    Debug.LogWarning("No selected game object to copy and paste coordinates to values above.");
                }
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        GUILayout.Label("Object Instantiation", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        {
            // instantiates the selected game object at vector3 zero
            if (GUILayout.Button("Instantiate selected game object at 0", GUILayout.ExpandWidth(false)))
            {
                if (Selection.activeGameObject)
                {
                    GameObject newlyInstantiatedGameObjectAt0 = Instantiate(Selection.activeGameObject.gameObject, Vector3.zero, Quaternion.identity);
                
                    Undo.RegisterCreatedObjectUndo(newlyInstantiatedGameObjectAt0, "Instantiated the selected game object at 0");
                
                    Selection.activeGameObject = newlyInstantiatedGameObjectAt0;
                }
                else
                {
                    Debug.LogWarning("No selection to Instantiate the selected game object.");
                }
            }
            
            // instantiates the selected game object using the advanced coordination
            if (GUILayout.Button("Instantiate selected game object using advanced coordination", GUILayout.ExpandWidth(false)))
            {
                if (Selection.activeGameObject)
                {
                    Vector3 modifiedPositionUsingAdvancedCoordination = new(advancedXPositionValue, advancedYPositionValue, advancedZPositionValue);
                    Quaternion modifiedRotation = Quaternion.Euler(advancedXRotationValue, advancedYRotationValue, advancedZRotationValue); 
                    
                    GameObject newlyInstantiatedGameObjectUsingAdvancedCoordination = Instantiate(Selection.activeGameObject.gameObject, modifiedPositionUsingAdvancedCoordination, modifiedRotation);
                    
                    Undo.RegisterCreatedObjectUndo(newlyInstantiatedGameObjectUsingAdvancedCoordination, "Instantiated selected game object with advanced coordination");
                    
                    Selection.activeGameObject = newlyInstantiatedGameObjectUsingAdvancedCoordination;
                }
                else
                {
                    Debug.LogWarning("No selection to Instantiate the selected game object.");
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private int newDotSizeForHeatMap;
    void LDVHeatMapConfigurations()
    {
        EditorGUILayout.HelpBox("There is not supposed to be more than 1 heat map at once!\n The heat map scripts are also supposed to be on planes!", MessageType.Info);
        
        GUILayout.BeginHorizontal();
        {
            // make a new plane become a heat map
            if (GUILayout.Button("Make This Plane A Heat Map"))
            {
                if (Selection.activeGameObject)
                {
                    // checks if exists, if not add, if does debug warning
                    HeatMapGeneration heatMapGeneration = Selection.activeGameObject.GetComponent<HeatMapGeneration>();
                    if (!heatMapGeneration)
                    {
                        heatMapGeneration = Selection.activeGameObject.AddComponent<HeatMapGeneration>();
                    }
                    else if (heatMapGeneration)
                    {
                        Debug.LogWarning("Game object already has the heatmap script attached.");
                    }
                }
                else
                {
                    Debug.LogWarning("No selection to add the heatmap script.");
                }
            }

            if (GUILayout.Button("Remove Heat Map script from this plane"))
            {
                if (Selection.activeGameObject)
                {
                    // checks if exists, if not debug warning, if does remove
                    HeatMapGeneration heatMapGeneration = Selection.activeGameObject.GetComponent<HeatMapGeneration>();
                    if (heatMapGeneration)
                    {
                        DestroyImmediate(heatMapGeneration, false);
                    }
                    else if (!heatMapGeneration)
                    {
                        Debug.LogWarning("Game object has no heatmap script to remove.");
                    }
                }
                else
                {
                    Debug.LogWarning("No selection to Remove the heatmap script.");
                }
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        {
            newDotSizeForHeatMap = (int)GUILayout.HorizontalSlider(newDotSizeForHeatMap, 0, 100);
            string inputNewDotValue = GUILayout.TextField(newDotSizeForHeatMap.ToString(), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Change Heat Map Dotting Size"))
            {
                if (Selection.activeGameObject)
                {
                    // change the size of the pen for this selected plane with heat map
                    if (Selection.activeGameObject.GetComponent<HeatMapGeneration>())
                    {
                        if (int.TryParse(inputNewDotValue, out int newDotSizeToChange))
                        {
                            Selection.activeGameObject.GetComponent<HeatMapGeneration>().heatMapPointSize = newDotSizeToChange;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No Heat map script found on selected game object.");
                    }
                }
                else
                {
                    Debug.LogWarning("No selected game object for this action");
                }
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Should be used in play mode otherwise might break", MessageType.Info);
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Make this the player for the heat map"))
            {
                // make the selected game object the new player
                if (Selection.activeGameObject)
                {
                    HeatMapGeneration[] gameObjectsWithHeatMapScripts = FindObjectsByType<HeatMapGeneration>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                    foreach (HeatMapGeneration gameObjectsWithHeatMap in gameObjectsWithHeatMapScripts)
                    {
                        // loop through each game object with the heatmap script and assign the selected game object to be the player
                        gameObjectsWithHeatMap.GetComponent<HeatMapGeneration>().playerGameObjectThatIsUsedForHeatMap = Selection.activeGameObject;
                    }
                }
                else
                {
                    Debug.LogWarning("No selection for this action");
                }
            }
        }
        GUILayout.EndHorizontal();
    }
}
