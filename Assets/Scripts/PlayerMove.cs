using System.Runtime.InteropServices;
using Mirror.BouncyCastle.Crypto.Engines;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Speed of the player movement
    [SerializeField] private float jump = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement(); // Call the movement handling method
        HandleJump();
        UpdateAmimation(); // Update the animation state based on movement and jumping
    }
    private void HandleMovement()
    {
        float moveInput=Input.GetAxis("Horizontal"); // Get horizontal input
        rb.linearVelocity= new Vector2 (moveInput* speed,rb.linearVelocity.y);
        if(moveInput>0) transform.localScale = new Vector3(5, 5, 5); // Face right
        else if (moveInput < 0) transform.localScale = new Vector3(-5, 5, 5); // Face left
        
    }
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump); // Apply upward force for jump
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer); // Check if the player is grounded
        
    }
    private void UpdateAmimation()
    {
        bool isRunning= Mathf.Abs(rb.linearVelocity.x) > 0.1f; // Check if the player is moving
        bool isJumping = !isGrounded; // Check if the player is jumping
        animator.SetBool("isRunning", isRunning); // Set the running animation state
        animator.SetBool("isJumping", isJumping);
    }
}
