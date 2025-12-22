using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewBiome", menuName = "World/Biome Data")]
public class BiomeData : ScriptableObject
{
    public string biomeName;
    
    [Header("Terrain Tile")]
    public UnityEngine.Tilemaps.TileBase groundTile;
    
    [Header("Path to LargeProps (eg. Assets/Tiles/Forest/LargeProps)")]
    public string treeFolderPath; 
    public UnityEngine.Tilemaps.TileBase[] treeTiles;

    [Header("Path to SmallProps (eg. Assets/Tiles/Forest/SmallProps)")]
    public string propsFolderPath;
    public UnityEngine.Tilemaps.TileBase[] propTiles;

    [Range(0, 1)] public float minHeight;

#if UNITY_EDITOR
    [ContextMenu("Auto Load Tiles From Folders")]
    public void LoadTiles()
    {
        if (!string.IsNullOrEmpty(treeFolderPath))
        {
            treeTiles = LoadTilesAtPath(treeFolderPath);
            Debug.Log($"Loaded {treeTiles.Length} to {biomeName}");
        }

        if (!string.IsNullOrEmpty(propsFolderPath))
        {
            propTiles = LoadTilesAtPath(propsFolderPath);
            Debug.Log($"Loaded {propTiles.Length} to {biomeName}");
        }
        
        EditorUtility.SetDirty(this);
    }

    private UnityEngine.Tilemaps.TileBase[] LoadTilesAtPath(string path)
    {
        string[] guids = AssetDatabase.FindAssets("t:TileBase", new[] { path });
        
        UnityEngine.Tilemaps.TileBase[] loadedTiles = new UnityEngine.Tilemaps.TileBase[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            loadedTiles[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>(assetPath);
        }
        return loadedTiles;
    }
#endif
}