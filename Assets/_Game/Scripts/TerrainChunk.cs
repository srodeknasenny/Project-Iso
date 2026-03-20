using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainChunk : MonoBehaviour
{
    [Header("Tilemapy Ziemi (Tylko z tego chunka)")]
    public Tilemap[] groundMaps; // [0] = Lvl0, [1] = Lvl1, itd.

    [Header("Foldery na Prefaby")]
    public Transform[] obstacleParents;
    public Transform[] decorationParents;

    private const int sortingMultiplier = 100;
    private const int elevationStep = 1000;

    // Funkcja odpalana przez Generator przy tworzeniu Chunka
    public void InitializeChunk(float maxWorldX, float maxWorldY, float maxWorldZ)
    {
        // Magia! Bierzemy "najdalszy" (najbardziej z tyłu) punkt chunka.
        // Odejmujemy 1 dla absolutnej pewności, że ziemia jest zawsze "ciut pod" obiektami.
        int baseOrder = Mathf.RoundToInt(-(maxWorldX + maxWorldY + maxWorldZ) * sortingMultiplier) - 1;

        for (int i = 0; i < groundMaps.Length; i++)
        {
            TilemapRenderer tmr = groundMaps[i].GetComponent<TilemapRenderer>();
            if (tmr != null)
            {
                // Nadajemy Order całej warstwie + bonus za jej wysokość!
                tmr.sortingOrder = baseOrder + (i * elevationStep);
            }
        }
    }
}