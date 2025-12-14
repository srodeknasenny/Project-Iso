using UnityEngine;
using UnityEngine.InputSystem; // Ważne dla nowego systemu!

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Ta funkcja jest wywoływana automatycznie przez komponent Player Input,
    // jeśli w Actions mapa nazywa się "Move" (standard w Unity).
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // 1. Wygładzanie ruchu (opcjonalne, ale przyjemniejsze dla oka)
        smoothedMovementInput = Vector2.SmoothDamp(
            smoothedMovementInput,
            moveInput,
            ref movementInputSmoothVelocity,
            0.1f);

        // 2. Obliczanie wektora ruchu
        // W izometrii czasem trzeba przesunąć wektory, ale na start zrobimy prosto:
        // W = Góra ekranu, S = Dół ekranu.
        Vector2 targetPosition = rb.position + smoothedMovementInput * moveSpeed * Time.fixedDeltaTime;

        // 3. Przesunięcie fizyczne
        rb.MovePosition(targetPosition);
    }
}