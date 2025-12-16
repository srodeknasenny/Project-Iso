using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private Vector2 ToIso(Vector2 input)
    {
        float x = input.x - input.y;
        float y = (input.x + input.y) / 2; 
        return new Vector2(x, y);
    }

    void FixedUpdate()
    {
        Vector2 isoInput = ToIso(moveInput);

        if (isoInput.magnitude > 1)
        {
            isoInput.Normalize();
        }

        smoothedMovementInput = Vector2.SmoothDamp(
            smoothedMovementInput,
            isoInput,
            ref movementInputSmoothVelocity,
            0.1f);

        Vector2 targetPosition = rb.position + smoothedMovementInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
        }
    }
}