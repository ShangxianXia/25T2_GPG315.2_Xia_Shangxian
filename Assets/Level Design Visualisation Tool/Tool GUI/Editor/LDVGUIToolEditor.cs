using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public class LDVGUIToolEditor : EditorWindow
{
    private enum WindowTabs {MainLDVMenu, LDVRuler, LDVGameObjectElements}
    private WindowTabs currentWindowTab = WindowTabs.MainLDVMenu;

    private static GameObject rulerSpherePrefab;
    private string rulerSpherePrefabFolder;

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
    }
    
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

    void LDVGameObjectModifiableElements()
    {
        
    }
}
