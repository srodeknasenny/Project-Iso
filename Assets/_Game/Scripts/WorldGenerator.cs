using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BiomePreset
{
    public string name;
    public TileBase groundTile; 
    
    // ZMIANA: Tablice zamiast pojedynczych zmiennych
    public TileBase[] treeTiles; // Tutaj wrzucisz wszystkie drzewa
    public TileBase[] propTiles; // Tutaj wrzucisz wszystkie kwiatki/krzaki
    
    [Range(0, 1)] public float minHeight;
}

public class WorldGenerator : MonoBehaviour
{
    [Header("Ustawienia Mapy")]
    public int width = 50;
    public int height = 50;
    public float scale = 0.1f;  // Skala szumu (im mniejsza, tym większe wyspy)
    public int seed = 12345;

    [Header("Referencje")]
    public Tilemap groundMap;   // Tu przypisz GroundMap
    public Tilemap objectMap;   // Tu przypisz ObjectMap

    [Header("Konfiguracja Biomów")]
    public BiomePreset[] biomes; // Lista biomów do ustawienia w Inspektorze

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Czyścimy mapy przed generowaniem
        groundMap.ClearAllTiles();
        objectMap.ClearAllTiles();

        // Jeśli seed jest 0, wylosuj go (dla losowości przy każdym starcie)
        if (seed == 0) seed = Random.Range(0, 100000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GenerateTile(x, y);
            }
        }
    }

    void GenerateTile(int x, int y)
    {
        // 1. Obliczamy Perlin Noise
        float xCoord = (float)x * scale + seed;
        float yCoord = (float)y * scale + seed;
        float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

        // 2. Wybieramy Biom
        BiomePreset currentBiome = ChooseBiome(noiseValue);

        // 3. Stawiamy kafelki
        // ... wewnątrz funkcji GenerateTile ...
        if (currentBiome != null)
        {
            Vector3Int pos = new Vector3Int(x, y, 0);
            
            // 1. Podłoga
            if (currentBiome.groundTile != null)
                groundMap.SetTile(pos, currentBiome.groundTile);

            // 2. Losowanie obiektów
            float chance = Random.value;

            // DRZEWA
            // Sprawdzamy czy mamy jakiekolwiek drzewa w tablicy (Length > 0)
            if (chance < 0.05f && currentBiome.treeTiles.Length > 0)
            {
                // Magiczna linijka: Wybierz losowy element z tablicy
                TileBase randomTree = currentBiome.treeTiles[Random.Range(0, currentBiome.treeTiles.Length)];
                objectMap.SetTile(pos, randomTree);
            }
            // PROPSY (KWIATKI/KRZAKI)
            else if (chance < 0.15f && currentBiome.propTiles.Length > 0)
            {
                TileBase randomProp = currentBiome.propTiles[Random.Range(0, currentBiome.propTiles.Length)];
                objectMap.SetTile(pos, randomProp);
            }
        }
    }

    BiomePreset ChooseBiome(float value)
    {
        // Pętla przechodzi przez listę biomów po kolei.
        // Ważne: W Inspektorze biomy muszą być posortowane od NAJMNIEJSZEGO progu do NAJWIĘKSZEGO.
        
        for (int i = 0; i < biomes.Length; i++)
        {
            if (value < biomes[i].minHeight)
            {
                return biomes[i];
            }
        }
        // Jeśli żaden nie pasował (np. wartość to 0.99, a ostatni biom ma 0.9), zwróć ostatni
        return biomes[biomes.Length - 1];
    }
}