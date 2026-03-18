using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewBiome", menuName = "World/Biome Data")]
public class BiomeData : ScriptableObject
{
    public string biomeName;
    
    [Header("Terrain")]
    public TileBase groundTile;
    
    [Header("OBSTACLES (Collision)")]
    public TileBase[] obstacleTiles;

    [Header("DECORATIONS (No collision)")]
    
    public TileBase[] decorationTiles;
    [Header("ELEVATION LEVEL")]
    public int elevationLevel;

    [Range(0, 1)] public float minHeight;
}