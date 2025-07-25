using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SyncVar]
    public bool isFinished = false;
    void Start()
    {
        // Disable movement if not local player
        if (!isLocalPlayer)
        {
            enabled = false;
            StopPlayerMovement();
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            CmdSetName("Player " + Random.Range(1000, 9999));
            //SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void Update()
    {
        InputCheck(); // Change scene to "Game" when the player is ready
    }

    private void InputCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Change scene to "Game"
            ResetGameRequest();
        }

        if (Input.GetKeyDown(KeyCode.F) && isLocalPlayer && !isFinished)
        {
            EnterFinishLine();
        }
    }

    public void StopPlayerMovement()
    {
        var playerMove = GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }
    }

    [Command]
    void CmdSetName(string name)
    {
        playerName = name;
    }

    [Client]
    void ResetGameRequest()
    {

    }

    [Client]
    void EnterFinishLine()
    {
        //DontDestroyOnLoad(this); // Ensure this object persists across scene changes

        connectionToServer.Send(new PlayerFinishLevelMessage { player = this.GameObject() });
    }
}
