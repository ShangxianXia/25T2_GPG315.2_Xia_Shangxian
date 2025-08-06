using UnityEngine;

public class HeatMapGeneration : MonoBehaviour
{
    [SerializeField] private Texture2D heatMapTextureReference;
    
    public int setTextureWidth = 512;
    [SerializeField] private int textureWidth = 512;
    
    public int setTextureHeight = 512;
    [SerializeField] private int textureHeight = 512;
    
    public GameObject playerGameObjectThatIsUsedForHeatMap;
    
    public GameObject settingTheBoundsOfTheHeatMapWithThisGameObjectReference;
    
    private Vector3 worldMinBounds;
    private Vector3 worldMaxBounds;

    private void Awake()
    {
        textureWidth = setTextureWidth;
        textureHeight = setTextureHeight;
        settingTheBoundsOfTheHeatMapWithThisGameObjectReference = gameObject;
        playerGameObjectThatIsUsedForHeatMap = GameObject.FindGameObjectWithTag("Player");
        
        // Calculate bounds based on reference object if one exists
        if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference != null)
        {
            CalculateBoundsFromReference();
        }
        else
        {
            Debug.LogWarning("No game object bound reference detected, line 26");
        }
    }
    
    private void CalculateBoundsFromReference()
    {
        if (!settingTheBoundsOfTheHeatMapWithThisGameObjectReference)
        {
            Debug.LogWarning("No game object bound reference detected, line 38");
            return;
        }

        // this trys to get the component of the renderer and set those as the bounds of the heatmap, most accurate
        if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Renderer>(out var renderer))
        {
            worldMinBounds = renderer.bounds.min;
            worldMaxBounds = renderer.bounds.max;
        }
        // this tries to use the collider of the game object this script is attached to
        else if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Collider>(out var collider))
        {
            worldMinBounds = collider.bounds.min;
            worldMaxBounds = collider.bounds.max;
        }

        // since y is for 2D only, this ignores y axis for now, also it makes the yellow gizmo show where it is if it were to be on the 0 axis
        worldMinBounds.y = 0;
        worldMaxBounds.y = 0;
    }

    private void Start()
    {
        ClearHeatMap();
    }
    
    void Update()
    {
        Vector3 playerPos = playerGameObjectThatIsUsedForHeatMap.transform.position;
        
        // Normalises world bound positions
        float normalizedX = Mathf.InverseLerp(worldMinBounds.x, worldMaxBounds.x, playerPos.x);
        float normalizedZ = Mathf.InverseLerp(worldMinBounds.z, worldMaxBounds.z, playerPos.z);
        
        // Map to texture coordinates
        int textureX = Mathf.FloorToInt(normalizedX * (textureWidth - 1));
        int textureY = Mathf.FloorToInt(normalizedZ * (textureHeight - 1));
        
        // Clamp values to ensure they stay within texture bounds
        textureX = Mathf.Clamp(textureX, 0, textureWidth - 1);
        textureY = Mathf.Clamp(textureY, 0, textureHeight - 1);
        
        heatMapTextureReference.SetPixel(textureX, textureY, Color.green);
        heatMapTextureReference.Apply();
    }
    
    public void ClearHeatMap()
    {
        heatMapTextureReference = new Texture2D(textureWidth, textureHeight);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                heatMapTextureReference.SetPixel(x, y, Color.black);
            }
        }
        heatMapTextureReference.Apply();
        
        GetComponent<MeshRenderer>().material.mainTexture = heatMapTextureReference;
    }
    
    // Helper method to visualize bounds in editor
    private void OnDrawGizmos()
    {
        if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference != null)
        {
            CalculateBoundsFromReference();
            Gizmos.color = Color.yellow;
            Vector3 center = (worldMaxBounds + worldMinBounds) / 2f;
            Vector3 size = worldMaxBounds - worldMinBounds;
            Gizmos.DrawWireCube(center, size);
        
            // Draw the reference object's bounds for comparison
            Gizmos.color = Color.red;
            if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Renderer>(out var renderer))
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
    }
}
