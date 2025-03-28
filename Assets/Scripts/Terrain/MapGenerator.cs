using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateMap();
    }

    public enum DrawMode
    {
        noiseMap, colorMap, Mesh, FallofMap
    };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public int octaves;
    [Range (0f, 1f)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFallof;

    public float meshHeightMultiplier;

    public TerrainType[] regions;

    public AnimationCurve meshHeightCurve;

    float[,] fallofMap;

    private void Awake()
    {
        fallofMap = FallofGenerator.GenerateFallofMap(mapWidth);
    }
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap (mapHeight, mapWidth, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y<mapHeight; y++)
        {   for(int x = 0; x<mapWidth; x++)
            {
                if (useFallof)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallofMap [x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for(int i=0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                    
                }
            }      

        }
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.noiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(drawMode == DrawMode.colorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        else if(drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            if (display.meshRenderer.gameObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            {
                DestroyImmediate(meshCollider);
            }
            display.meshRenderer.gameObject.AddComponent<MeshCollider>();
        }
        else if(drawMode == DrawMode.FallofMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallofGenerator.GenerateFallofMap(mapWidth)));
        }
    }

    void OnValidate()
    {
        if(mapHeight < 1)
        {
            mapHeight = 1;
        }
        if(mapWidth < 1)
        {
            mapWidth = 1;
        }
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }
        if(octaves < 0)
        {
            octaves = 0;
        }
        fallofMap = FallofGenerator.GenerateFallofMap(mapWidth);
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
