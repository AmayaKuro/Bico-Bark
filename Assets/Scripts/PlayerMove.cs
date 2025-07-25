using Mirror;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Speed of the player movement
    [SerializeField] private float jump = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform playerCheck;
    private bool isPlayer;
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private float lastJumpTime = -1.5f; // Track the last jump time
    private const float jumpCooldown = 1.5f; // 1 second cooldown

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement(); // Call the movement handling method
        HandleJump();
        UpdateAmimation(); // Update the animation state based on movement and jumping
        PlayerJump();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal"); // Get horizontal input
        Vector2 newVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Get camera boundaries in world space
        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        // Get player width (assuming pivot is center)
        float halfWidth = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>().bounds.extents.x : 0.5f;

        // Clamp position after movement
        float nextX = Mathf.Clamp(transform.position.x + newVelocity.x * Time.fixedDeltaTime, min.x + halfWidth, max.x - halfWidth);
        rb.linearVelocity = new Vector2((nextX - transform.position.x) / Time.fixedDeltaTime, newVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(5, 5, 5); // Face right
        else if (moveInput < 0) transform.localScale = new Vector3(-5, 5, 5); // Face left
    }

    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer); // Check if the player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump); // Apply upward force for jump
            lastJumpTime = Time.time; // Update last jump time
        }
    }
    private void PlayerJump()
    {
        // Check if the player is standing on top of another player
        isPlayer = Physics2D.OverlapCircle(playerCheck.position, 0.1f, playerLayer);

        // Only allow jump if this is the local player and is standing on another player
        if (Input.GetButtonDown("Jump") && isPlayer && Time.time - lastJumpTime >= jumpCooldown)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            lastJumpTime = Time.time; // Update last jump time
        }
    }

    private void UpdateAmimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f; // Check if the player is moving
        bool isJumping = !isGrounded; // Check if the player is jumping
        animator.SetBool("isRunning", isRunning); // Set the running animation state
        animator.SetBool("isJumping", isJumping);
    }
}
