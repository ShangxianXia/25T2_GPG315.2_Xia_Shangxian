using System.Transactions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HeatMapGeneration : MonoBehaviour
{
    [Header("Player Reference")] [Space(5)]
    public PrefabHolderIfPlayerIsNotAssigned playerPrefabHolderReference;
    public GameObject playerGameObjectThatIsUsedForHeatMap;
    public bool shownMissingReferenceMessage;
    
    [Header("Heat Map size/reference")] [Space(5)]
    [SerializeField] private Texture2D heatMapTextureReference;
    
    public int setTextureWidth = 512;
    [SerializeField] private int textureWidth = 512;
    
    public int setTextureHeight = 512;
    [SerializeField] private int textureHeight = 512;
    
    [Header("Heat map, point size / colour")] [Space(5)]
    public int setHeatMapPointSize = 4;
    public int heatMapPointSize = 4;
    [SerializeField] private Color heatMapColour = Color.green;
    private Color modifiedHeatMapColour;
    
    // heat map boundaries
    public GameObject settingTheBoundsOfTheHeatMapWithThisGameObjectReference;
    public Vector3 worldMinBounds;
    public Vector3 worldMaxBounds;

    private void Awake()
    {
        gameObject.tag = "HeatMap";
        playerPrefabHolderReference = FindFirstObjectByType<PrefabHolderIfPlayerIsNotAssigned>();
        textureWidth = setTextureWidth;
        textureHeight = setTextureHeight;
        settingTheBoundsOfTheHeatMapWithThisGameObjectReference = gameObject;
        heatMapPointSize = setHeatMapPointSize;
        modifiedHeatMapColour = heatMapColour;
        modifiedHeatMapColour.a = 0;
        modifiedHeatMapColour.r = 0;
        modifiedHeatMapColour.g = 0.01f;

        if (!playerGameObjectThatIsUsedForHeatMap)
        {
            Debug.LogWarning("PlayerGameObjectThatIsUsedForHeatMap is null, trying to find a new player");
            playerGameObjectThatIsUsedForHeatMap = GameObject.FindGameObjectWithTag("Player");
        }

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
        if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Renderer>(out var rendererOfTheGameObjectBeingUsed))
        {
            worldMinBounds = rendererOfTheGameObjectBeingUsed.bounds.min;
            worldMaxBounds = rendererOfTheGameObjectBeingUsed.bounds.max;
        }
        // this tries to use the collider of the game object this script is attached to
        else if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Collider>(out var colliderOfTheGameObjectBeingUsed))
        {
            worldMinBounds = colliderOfTheGameObjectBeingUsed.bounds.min;
            worldMaxBounds = colliderOfTheGameObjectBeingUsed.bounds.max;
        }

        // since y is for 2D only, this ignores y axis for now, also it makes the yellow gizmo show where it is if it were to be on the 0 axis
        worldMinBounds.y = 0;
        worldMaxBounds.y = 0;
    }

    private void Start()
    {
        if ((int)gameObject.transform.rotation.y != 180)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (!playerPrefabHolderReference)
        {
            playerPrefabHolderReference = FindFirstObjectByType<PrefabHolderIfPlayerIsNotAssigned>();
        }
        ClearHeatMap();
    }
    
    void Update()
    {
        PlotHeatPointsOnMapWherePlayerIsOnMap();
    }
    
    private void ClearHeatMap()
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
        
        MeshRenderer meshRendererReference = GetComponent<MeshRenderer>();
        meshRendererReference.material.shader = Shader.Find("Unlit/Texture");
        meshRendererReference.material.mainTexture = heatMapTextureReference;
    }

    private void PlotHeatPointsOnMapWherePlayerIsOnMap()
    {
        if (!playerGameObjectThatIsUsedForHeatMap && !shownMissingReferenceMessage)
        {
            SpawnNewTempPlayerIfNonExists();
        }

        Vector3 playerPos = playerGameObjectThatIsUsedForHeatMap.transform.position;
        
        // Normalises world bound positions
        float normalizedX = Mathf.InverseLerp(worldMinBounds.x, worldMaxBounds.x, playerPos.x);
        float normalizedZ = Mathf.InverseLerp(worldMinBounds.z, worldMaxBounds.z, playerPos.z);

        if (normalizedX < 0 || normalizedX > 1 || normalizedZ < 0 || normalizedZ > 1)
        {
            return;
        }
        
        // Map to texture coordinates
        int textureX = Mathf.FloorToInt(normalizedX * (textureWidth - 1));
        int textureY = Mathf.FloorToInt(normalizedZ * (textureHeight - 1));

        if (textureX >= 0 && textureX < textureWidth && textureY >= 0 && textureY < textureHeight)
        {
            // Clamp values to ensure they stay within texture bounds
            // textureX = Mathf.Clamp(textureX, 0, textureWidth - 1);
            // textureY = Mathf.Clamp(textureY, 0, textureHeight - 1);


            for (int y = -heatMapPointSize; y <= heatMapPointSize; y++)
            {
                for (int x = -heatMapPointSize; x <= heatMapPointSize; x++)
                {
                    if (Mathf.Sqrt(x * x + y * y) <= heatMapPointSize)
                    {
                        Color currentColour = heatMapTextureReference.GetPixel(textureX + x, textureY + y);
                        currentColour += new Color(0, modifiedHeatMapColour.g, 0);

                        heatMapTextureReference.SetPixel(textureX + x, textureY + y, currentColour);
                    }
                }
            }

            heatMapTextureReference.Apply();
        }
    }
    
    // A visualisation of the boundaries in editor scene, a sort of help
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
            if (settingTheBoundsOfTheHeatMapWithThisGameObjectReference.TryGetComponent<Renderer>(out var rendererOfTheGameObjectBeingUsed))
            {
                Gizmos.DrawWireCube(rendererOfTheGameObjectBeingUsed.bounds.center, rendererOfTheGameObjectBeingUsed.bounds.size);
            }
        }
    }

    private void SpawnNewTempPlayerIfNonExists()
    {
        shownMissingReferenceMessage = true;
        Debug.LogWarning("No player for heat map plotting detected, spawning a new player on top");
        Vector3 modifiedPositionForPlayer = gameObject.transform.position;
        modifiedPositionForPlayer.y += 1f;
        playerGameObjectThatIsUsedForHeatMap = Instantiate(playerPrefabHolderReference.playerPrefabGameObjectHolder, modifiedPositionForPlayer, Quaternion.identity);
        playerGameObjectThatIsUsedForHeatMap.tag = "Player";
        shownMissingReferenceMessage = false;
    }
}
