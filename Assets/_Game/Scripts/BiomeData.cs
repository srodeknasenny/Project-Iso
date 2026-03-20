using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBiome", menuName = "World/Biome Data")]
public class BiomeData : ScriptableObject
{
    public string biomeName;
    public int elevationLevel; 

    [Header("Terrain")]
    public TileBase topTile;  
    public TileBase fillTile; 
    
    [Header("OBSTACLES (Prefaby)")]
    public GameObject[] obstaclePrefabs; 

    [Header("DECORATIONS (Prefaby)")]
    public GameObject[] decorationPrefabs; 

    [Range(0, 1)] public float minHeight; 
}