using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.U2D.Sprites; // WAŻNE: Nowy namespace do obsługi Slicingu
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProjectPipeline : AssetPostprocessor
{

    void DistributeTerrain(Sprite[] sprites)
    {
        AssignGround(sprites, 0, "Ocean", "Ocean"); 
        AssignGround(sprites, 1, "Beach", "Beach");
        AssignGround(sprites, 2, "Meadow", "Meadow");
    }

    void DistributePropsSmall(Sprite[] sprites)
    {
        AssignRange(sprites, 0, 2, "Meadow", "Ocean", isObstacle: false);
        AssignRange(sprites, 3, 6, "Beach", "Beach", isObstacle: false);
        AssignRange(sprites, 7, 12, "Meadow", "Meadow", isObstacle: false);
        AssignRange(sprites, 13, 14, "Meadow", "Meadow", isObstacle: true);
    }

    void DistributePropsLarge(Sprite[] sprites)
    {
        AssignRange(sprites, 0, 0, "Beach", "Beach", isObstacle: true);
        AssignRange(sprites, 1, 2, "Meadow", "Meadow", isObstacle: true);
    }

    void OnPreprocessTexture()
    {
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        if (!assetPath.StartsWith("Assets/")) return;

        int sliceW = 32; 
        int sliceH = 32; 
        SpriteAlignment pivot = SpriteAlignment.Center;

        if (fileName == "Terrain")
        {
            sliceW = 32; sliceH = 32;
            pivot = SpriteAlignment.Center;
        }
        else if (fileName == "Props_Small")
        {
            sliceW = 32; sliceH = 32;
            pivot = SpriteAlignment.BottomCenter;
        }
        else if (fileName == "Props_Large")
        {
            sliceW = 32; sliceH = 64;
            pivot = SpriteAlignment.BottomCenter;
        }
        else
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        SliceTextureAutomatically(importer, sliceW, sliceH, pivot);
    }

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (string path in imported)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            if (fileName == "Terrain" || fileName == "Props_Small" || fileName == "Props_Large")
            {
                var script = new ProjectPipeline();
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                Sprite[] sprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToArray();

                if (fileName == "Terrain") script.DistributeTerrain(sprites);
                else if (fileName == "Props_Small") script.DistributePropsSmall(sprites);
                else if (fileName == "Props_Large") script.DistributePropsLarge(sprites);
                
                Debug.Log($"✅ <color=green>Pipeline:</color> Przetworzono i rozdano kafelki z {fileName}!");
            }
        }
    }

    void AssignGround(Sprite[] sprites, int index, string biomeName, string assetNameSearch)
    {
        if (index >= sprites.Length) return;
        
        BiomeData biome = FindBiomeData(assetNameSearch);
        if (biome != null)
        {
            Tile tile = GetOrCreateTile(sprites[index], biomeName, "Ground");
            biome.groundTile = tile;
            EditorUtility.SetDirty(biome);
        }
    }

    void AssignRange(Sprite[] sprites, int startIdx, int endIdx, string biomeName, string assetNameSearch, bool isObstacle)
    {
        BiomeData biome = FindBiomeData(assetNameSearch);
        if (biome == null) return;

        List<TileBase> tilesList = new List<TileBase>();
        
        for (int i = startIdx; i <= endIdx; i++)
        {
            if (i < sprites.Length)
            {
                string cat = isObstacle ? "Obstacles" : "Decorations";
                Tile tile = GetOrCreateTile(sprites[i], biomeName, cat);
                tilesList.Add(tile);
            }
        }

        List<TileBase> currentList = new List<TileBase>();
        if (isObstacle && biome.obstacleTiles != null) currentList.AddRange(biome.obstacleTiles);
        else if (!isObstacle && biome.decorationTiles != null) currentList.AddRange(biome.decorationTiles);

        foreach (var t in tilesList)
        {
            if (!currentList.Contains(t)) currentList.Add(t);
        }

        if (isObstacle) biome.obstacleTiles = currentList.ToArray();
        else biome.decorationTiles = currentList.ToArray();

        EditorUtility.SetDirty(biome);
    }

    Tile GetOrCreateTile(Sprite sprite, string folderName, string subfolder)
    {
        string folderPath = $"Assets/Tiles/{folderName}/{subfolder}";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string tilePath = $"{folderPath}/{sprite.name}.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);

        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        else
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    BiomeData FindBiomeData(string searchName)
    {
        string[] guids = AssetDatabase.FindAssets($"t:BiomeData {searchName}");
        if (guids.Length == 0) 
        {
            Debug.LogError($"❌ Nie znaleziono BiomData o nazwie: {searchName}");
            return null;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<BiomeData>(path);
    }
    void SliceTextureAutomatically(TextureImporter importer, int w, int h, SpriteAlignment align)
    {
        Texture2D texture = LoadTextureRaw(importer.assetPath);
        if (texture == null) return;

        int width = texture.width;
        int height = texture.height;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = new List<SpriteRect>();
        int cols = width / w;
        int rows = height / h;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Rect rect = new Rect(x * w, height - (y + 1) * h, w, h);

                if (IsTileEmpty(texture, rect))
                {
                    continue;
                }

                var spriteRect = new SpriteRect();
                spriteRect.rect = rect;
                spriteRect.alignment = align;
                spriteRect.pivot = GetPivotValue(align);

                int visualIndex = y * cols + x;
                spriteRect.name = $"{Path.GetFileNameWithoutExtension(importer.assetPath)}_{visualIndex}";
                spriteRect.spriteID = GUID.Generate();
                
                spriteRects.Add(spriteRect);
            }
        }

        Object.DestroyImmediate(texture);

        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();
    }

    bool IsTileEmpty(Texture2D tex, Rect rect)
    {
        try
        {
            Color[] pixels = tex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

            foreach (Color p in pixels)
            {
                if (p.a > 0.01f)
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    Texture2D LoadTextureRaw(string path)
    {
        if (!File.Exists(path)) return null;
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

    Vector2 GetPivotValue(SpriteAlignment alignment)
    {
        switch (alignment)
        {
            case SpriteAlignment.Center: return new Vector2(0.5f, 0.5f);
            case SpriteAlignment.TopCenter: return new Vector2(0.5f, 1f);
            case SpriteAlignment.BottomCenter: return new Vector2(0.5f, 0f);
            case SpriteAlignment.LeftCenter: return new Vector2(0f, 0.5f);
            case SpriteAlignment.RightCenter: return new Vector2(1f, 0.5f);
            case SpriteAlignment.TopLeft: return new Vector2(0f, 1f);
            case SpriteAlignment.TopRight: return new Vector2(1f, 1f);
            case SpriteAlignment.BottomLeft: return new Vector2(0f, 0f);
            case SpriteAlignment.BottomRight: return new Vector2(1f, 0f);
            default: return new Vector2(0.5f, 0.5f);
        }
    }

    static void GetImageSize(string path, out int width, out int height)
    {
        width = 0; height = 0;
        try {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (var br = new BinaryReader(fs)) {
                    fs.Seek(16, SeekOrigin.Begin);
                    var wBytes = br.ReadBytes(4);
                    var hBytes = br.ReadBytes(4);
                    if (System.BitConverter.IsLittleEndian) { System.Array.Reverse(wBytes); System.Array.Reverse(hBytes); }
                    width = System.BitConverter.ToInt32(wBytes, 0);
                    height = System.BitConverter.ToInt32(hBytes, 0);
                }
            }
        } catch { }
    }
}