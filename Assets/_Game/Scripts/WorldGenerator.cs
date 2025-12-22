using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int width = 50;
    public int height = 50;
    public float scale = 0.1f;
    public int seed = 12345;

    [Header("References")]
    public Tilemap groundMap;
    public Tilemap objectMap;

    [Header("Data")]
    public BiomeData[] biomes; 

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        groundMap.ClearAllTiles();
        objectMap.ClearAllTiles();
        if (seed == 0) seed = Random.Range(0, 100000);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                GenerateTile(x, y);
    }

    void GenerateTile(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise((float)x * scale + seed, (float)y * scale + seed);
        BiomeData biome = ChooseBiome(noiseValue);

        if (biome == null) return;

        Vector3Int pos = new Vector3Int(x, y, 0);

        if (biome.groundTile != null)
        {
            groundMap.SetTile(pos, biome.groundTile);
        }

        float chance = Random.value; 

        if (chance < 0.05f) 
        {
            if (biome.treeTiles != null && biome.treeTiles.Length > 0)
            {
                TileBase treeToPlace = biome.treeTiles[Random.Range(0, biome.treeTiles.Length)];
                objectMap.SetTile(pos, treeToPlace);
                
                return;
            }
        }

        else if (chance < 0.15f) 
        {
            if (biome.propTiles != null && biome.propTiles.Length > 0)
            {
                TileBase propToPlace = biome.propTiles[Random.Range(0, biome.propTiles.Length)];
                objectMap.SetTile(pos, propToPlace);
                
                return;
            }
        }
        
    }

    BiomeData ChooseBiome(float value)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            if (value < biomes[i].minHeight) return biomes[i];
        }
        return biomes[biomes.Length - 1];
    }
}