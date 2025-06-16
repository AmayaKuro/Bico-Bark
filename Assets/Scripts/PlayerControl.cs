using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    void Start()
    {
        // Disable movement if not local player
        if (!isLocalPlayer)
        {
            enabled = false;
            var playerMove = GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.enabled = false;
            }
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
        ResetGame(); // Change scene to "Game" when the player is ready
    }

    private void ResetGame()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Change scene to "Game"
            ChangeScene(SceneManager.GetActiveScene().name);
        }
    }

    [Command]
    void CmdSetName(string name)
    {
        playerName = name;
    }

    [Command(requiresAuthority = false)]
    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
