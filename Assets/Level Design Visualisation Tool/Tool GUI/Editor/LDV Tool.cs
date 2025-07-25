using UnityEditor;
using UnityEngine;

public class LDVTool : EditorWindow
{
    private enum WindowTabs {MainLDVTab, LDVRuler}
    private WindowTabs currentWindowTab = WindowTabs.MainLDVTab;

    [MenuItem("LDV/Level Design Visualisation Tool")]
    public static void ShowLDVToolWindow()
    {
        GetWindow<LDVTool>("Level Design Visualisation Tool");
    }

    void OnGUI()
    {
        currentWindowTab = (WindowTabs)GUILayout.Toolbar((int)currentWindowTab, System.Enum.GetNames(typeof(WindowTabs)));

        switch (currentWindowTab)
        {
            case WindowTabs.MainLDVTab:
                MainLDVTabStuff();
                break;
            
            case WindowTabs.LDVRuler:
                LDVRulerStuff();
                break;
        }
    }

    void MainLDVTabStuff()
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
    }

    void LDVRulerStuff()
    {
        
    }
}
