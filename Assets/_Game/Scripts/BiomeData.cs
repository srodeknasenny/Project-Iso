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
    public string obstaclePath; 
    public TileBase[] obstacleTiles;

    [Header("DECORATIONS (No collision)")]
    public string decorationPath; 
    public TileBase[] decorationTiles;

    [Range(0, 1)] public float minHeight;


#if UNITY_EDITOR
    [ContextMenu("Auto Load Tiles From Folders")]
    public void LoadTiles()
    {
        if (!string.IsNullOrEmpty(obstaclePath))
        {
            obstacleTiles = LoadTilesAtPath(obstaclePath);
            Debug.Log($"Loaded {obstacleTiles.Length} obstacles to {biomeName}");
        }

        if (!string.IsNullOrEmpty(decorationPath))
        {
            decorationTiles = LoadTilesAtPath(decorationPath);
            Debug.Log($"Loaded {decorationTiles.Length} decorations to {biomeName}");
        }
        
        EditorUtility.SetDirty(this);
    }

    private UnityEngine.Tilemaps.TileBase[] LoadTilesAtPath(string path)
    {
        if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"ERROR: Path problems on: '{path}'");
            return new UnityEngine.Tilemaps.TileBase[0];
        }

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