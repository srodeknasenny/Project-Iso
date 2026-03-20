using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public bool isStatic = true;

    [HideInInspector] public int elevationLevel;
    private const int sortingMultiplier = 100;
    private const int elevationStep = 1000;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        UpdateSorting();
    }
    void Update()
    {
        UpdateSorting();
    }
    void UpdateSorting()
    {
        int calculatedOrder = Mathf.RoundToInt(- (transform.position.x + transform.position.y + transform.position.z)*sortingMultiplier);
        calculatedOrder += (elevationLevel * elevationStep);
        spriteRenderer.sortingOrder = calculatedOrder;
    }   
}