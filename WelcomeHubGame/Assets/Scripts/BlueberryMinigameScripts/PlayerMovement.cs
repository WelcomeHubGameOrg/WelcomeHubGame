using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // Set to false (e.g. by BushUIWindow) to freeze movement and jumping while a UI panel is open
    public static bool InputEnabled = true;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [Tooltip("Logs jump attempts and ground-state changes to the Console. Turn off once jumping works.")]
    [SerializeField] private bool debugLogging = true;

    private Rigidbody2D rb;
    private float moveInputX;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (groundCheck == null)
            Debug.LogWarning($"[{nameof(PlayerMovement)}] Ground Check is not assigned in the Inspector — jumping and ground detection will not work.", this);

        if (debugLogging)
            Debug.Log($"[{nameof(PlayerMovement)}] Start: groundCheck={(groundCheck != null ? groundCheck.name : "NULL")}, " +
                      $"groundCheckRadius={groundCheckRadius}, groundLayer mask={groundLayer.value}, bodyType={rb.bodyType}, gravityScale={rb.gravityScale}");
    }

    void Update()
    {
        if (!InputEnabled)
        {
            // A UI panel (e.g. the bush berry window) is open — ignore movement/jump input
            moveInputX = 0f;
            return;
        }

        // Horizontal movement (A/D, arrow keys)
        moveInputX = Input.GetAxisRaw("Horizontal");

        // Jump on Space, only while grounded
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (debugLogging)
                    Debug.Log($"[{nameof(PlayerMovement)}] Jump! isGrounded=true, applied jumpForce={jumpForce}");
            }
            else if (debugLogging)
            {
                Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position;
                Debug.Log($"[{nameof(PlayerMovement)}] Jump blocked: isGrounded=false. " +
                          $"groundCheck world position={checkPos}, radius={groundCheckRadius}, groundLayer mask={groundLayer.value}");
            }
        }
    }

    void FixedUpdate()
    {
        // Ground check: physics OverlapCircle at the groundCheck point, filtered by groundLayer
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (debugLogging && isGrounded != wasGrounded)
        {
            Debug.Log($"[{nameof(PlayerMovement)}] isGrounded changed: {wasGrounded} -> {isGrounded} " +
                      $"(groundCheck world position={(groundCheck != null ? groundCheck.position : transform.position)})");
            wasGrounded = isGrounded;
        }

        // Horizontal velocity comes from input; vertical velocity is left alone — gravity and jump control it
        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Green while grounded, red while airborne, so the ground check state is visible in the Scene view
            Gizmos.color = Application.isPlaying ? (isGrounded ? Color.green : Color.red) : Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
