using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int chunksX = 3;
    public int chunksY = 3;
    public TerrainChunk chunkPrefab; 
    public Grid mainGrid;

    [Header("World Settings")]
    public float scale = 0.1f;
    public int seed = 12345;

    [Header("Data")]
    public BiomeData[] biomes; 

    private List<TerrainChunk> spawnedChunks = new List<TerrainChunk>();

    void Start()
    {
        if (mainGrid == null) mainGrid = GetComponent<Grid>();
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        foreach(TerrainChunk chunk in spawnedChunks)
        {
            if (chunk != null) Destroy(chunk.gameObject);
        }
        spawnedChunks.Clear();

        if (seed == 0) seed = Random.Range(0, 100000);

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                BuildChunk(cx, cy);
            }
        }
    }

    void BuildChunk(int chunkX, int chunkY)
    {
        Vector3Int chunkBaseCoords = new Vector3Int(chunkX * chunkSize, chunkY * chunkSize, 0);
        Vector3 chunkWorldPos = mainGrid.CellToWorld(chunkBaseCoords);

        TerrainChunk chunk = Instantiate(chunkPrefab, chunkWorldPos, Quaternion.identity, transform);
        chunk.name = $"Chunk_{chunkX}_{chunkY}";
        spawnedChunks.Add(chunk);

        Vector3Int maxCoords = new Vector3Int((chunkX + 1) * chunkSize, (chunkY + 1) * chunkSize, 0);
        Vector3 maxWorldPos = mainGrid.CellToWorld(maxCoords);
        

        for (int lx = 0; lx < chunkSize; lx++)
        {
            for (int ly = 0; ly < chunkSize; ly++)
            {
                int globalX = (chunkX * chunkSize) + lx;
                int globalY = (chunkY * chunkSize) + ly;

                GenerateTileInChunk(chunk, lx, ly, globalX, globalY);
            }
        }
    }

    void GenerateTileInChunk(TerrainChunk chunk, int localX, int localY, int globalX, int globalY)
    {
        float noiseValue = Mathf.PerlinNoise((float)globalX * scale + seed, (float)globalY * scale + seed);
        BiomeData biome = ChooseBiome(noiseValue);

        if (biome == null) return;

        Vector3Int localPos = new Vector3Int(localX, localY, 0);

        int elevationLevel = biome.elevationLevel;
        if (elevationLevel >= chunk.groundMaps.Length) elevationLevel = chunk.groundMaps.Length - 1;

        for(int i = 0; i < elevationLevel; i++)
        {
            chunk.groundMaps[i].SetTile(localPos, biome.fillTile); 
        }
        chunk.groundMaps[elevationLevel].SetTile(localPos, biome.topTile);

        Vector3 worldPos = chunk.groundMaps[elevationLevel].GetCellCenterWorld(localPos);
        float chance = Random.value; 
        bool placedObstacle = false;
        
        if (chance < 0.05f && biome.obstaclePrefabs != null && biome.obstaclePrefabs.Length > 0)
        {
            GameObject prefab = biome.obstaclePrefabs[Random.Range(0, biome.obstaclePrefabs.Length)];
            
            GameObject newObj = Instantiate(prefab, worldPos, Quaternion.identity, chunk.obstacleParents[elevationLevel]);
            
            DepthSorter sorter = newObj.GetComponent<DepthSorter>();
            if (sorter != null) sorter.elevationLevel = elevationLevel;
            
            placedObstacle = true;
            return; 
        }

        float decorationChance = Random.value; 
        if (!placedObstacle && decorationChance < 0.2f && biome.decorationPrefabs != null && biome.decorationPrefabs.Length > 0)
        {
            GameObject prefab = biome.decorationPrefabs[Random.Range(0, biome.decorationPrefabs.Length)];
            GameObject newObj = Instantiate(prefab, worldPos, Quaternion.identity, chunk.decorationParents[elevationLevel]);
            
            DepthSorter sorter = newObj.GetComponent<DepthSorter>();
            if (sorter != null) sorter.elevationLevel = elevationLevel;
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