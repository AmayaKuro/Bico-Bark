using Mirror;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SerializeField] private float speed = 5f; // Speed of the player movement
    [SerializeField] private float jump = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Disable movement if not local player
        if (!isLocalPlayer)
        {
            enabled = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            CmdSetName("Player " + Random.Range(1000, 9999));
        }
    }

    //[Client]
    //public void StartGame()
    //{
    //    CmdRequestSceneChange("Scenes/Game");
    //}

    //[Command]
    //private void CmdRequestSceneChange(string sceneName)
    //{
    //    Debug.Log($"Requesting scene change to {sceneName} from {playerName}");
    //    if (isServer)
    //    {
    //        if (NetworkManager.singleton == null)
    //        {
    //            Debug.LogError("NetworkManager.singleton is null!");
    //            return;
    //        }

    //        NetworkManager.singleton.ServerChangeScene(sceneName);
    //    }
    //}


    void Update()
    {
        HandleMovement(); // Call the movement handling method
        HandleJump();
        UpdateAmimation(); // Update the animation state based on movement and jumping
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
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump); // Apply upward force for jump
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer); // Check if the player is grounded

    }

    private void UpdateAmimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f; // Check if the player is moving
        bool isJumping = !isGrounded; // Check if the player is jumping
        animator.SetBool("isRunning", isRunning); // Set the running animation state
        animator.SetBool("isJumping", isJumping);
    }

    [Command]
    void CmdSetName(string name)
    {
        playerName = name;
    }
}
