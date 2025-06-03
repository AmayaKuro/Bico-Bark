using Mirror;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        // Handle player movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    [Command]
    void CmdSetName(string name)
    {
        playerName = name;
    }
}
