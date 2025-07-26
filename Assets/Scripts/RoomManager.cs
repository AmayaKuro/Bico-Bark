using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class RoomManager : NetworkRoomManager
{
    public List<GameObject> players = new List<GameObject>();
    // Add this field to track finished players
    private HashSet<uint> finishedPlayers = new HashSet<uint>();

    [Tooltip("Reference to FadeInOut script on child FadeCanvas")]
    public FadeInOut fadeInOut;
    bool isInTransition;

    [Header("MultiScene Setup")]
    public int instances = 3;
    [Scene, Tooltip("Add additive scenes here.\nFirst entry will be players' start scene")]
    public List<string> subGameScenes = new List<string>();
    public int currentLevelIndex = 0;

    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new RoomManager singleton => (RoomManager)NetworkRoomManager.singleton;

    #region Server Callbacks

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        //base.OnRoomServerSceneChanged(sceneName);

        // Only subscribe in game scenes, not in the room scene
        if (subGameScenes.Contains(sceneName))
        {
            var startPosition = GameObject.Find("Start")?.GetComponent<Transform>();
            if (startPosition != null)
            {
                RegisterStartPosition(startPosition);
            }

            // Register the handler for PlayerFinishLevelMessage
            NetworkServer.RegisterHandler<PlayerFinishLevelMessage>(OnPlayerFinishLevelMessageReceived, false);
            NetworkServer.RegisterHandler<PlayerFailMessage>(OnPlayerFailMessageReceived, false);


            Debug.Log("Subscribed to PlayerFinishLevelMessage in scene: " + sceneName);
        }
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        // Remove the roomPlayer object when switching to the game scene
        if (roomPlayer != null)
        {
            NetworkServer.Destroy(roomPlayer);
        }

        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        if (subGameScenes != null && subGameScenes.Count > 0)
        {
            string selectedScene = subGameScenes[currentLevelIndex];
            ServerChangeScene(selectedScene);

            Debug.Log($"Loading sub-game scene: {selectedScene}");
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (subGameScenes.Contains(SceneManager.GetActiveScene().path))
        {
            allPlayersReady = false;

            Transform startPos = GetStartPosition();
            var currentPlayerObject = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            NetworkServer.ReplacePlayerForConnection(conn, currentPlayerObject, ReplacePlayerOptions.KeepAuthority);
        }
        else
        {
            base.OnServerAddPlayer(conn);
        }
    }

    #endregion

    #region Client Callbacks

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    //public override void OnRoomClientSceneChanged()
    //{
    //    base.OnRoomClientSceneChanged();

    //    // Ensure the client is marked as ready after a scene change
    //    if (!NetworkClient.ready)
    //    {
    //        NetworkClient.Ready();
    //    }
    //}


    /// <summary>
    /// Client scene will fade if change game scene 
    /// </summary
    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (subGameScenes.Contains(sceneName) || sceneName == GameplayScene)
        {
            StartCoroutine(FadeLoadScene(sceneName));
        }
        else
        {
            base.OnClientChangeScene(sceneName, sceneOperation, customHandling);
        }
    }

    IEnumerator FadeLoadScene(string sceneName)
    {
        isInTransition = true;

        // Disable client movement during transition
        NetworkClient.localPlayer?.GetComponent<PlayerControl>().StopPlayerMovement();

        // This will return immediately if already faded in
        // e.g. by UnloadAdditive or by default startup state
        yield return fadeInOut.FadeIn();

        // Start loading the additive subscene
        loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName);

        while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
            yield return null;

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        OnClientSceneChanged();

        // Reveal the new scene content.
        yield return fadeInOut.FadeOut();
    }

    public override void OnClientSceneChanged()
    {
        // Only call the base method if not in a transition.
        // This will be called from LoadAdditive / UnloadAdditive after setting isInTransition to false
        // but will also be called first by Mirror when the scene loading finishes.
        if (!isInTransition)
            base.OnClientSceneChanged();
    }
    #endregion

    #region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion

    #region Server Listeners

    private void OnPlayerFinishLevelMessageReceived(NetworkConnectionToClient conn, PlayerFinishLevelMessage msg)
    {
        string currentScene = SceneManager.GetActiveScene().path;
        Debug.Log($"Received PlayerFinishLevelMessage from {conn.identity} in scene {currentScene}");

        if (!subGameScenes.Contains(currentScene))
            return;

        // Track by NetworkIdentity netId for uniqueness
        //uint netId = conn.identity != null ? conn.identity.netId : 0;
        //if (netId == 0)
        //    return;

        //finishedPlayers.Add(netId);

        Debug.Log($"Player {conn.identity} finished the level in scene {currentScene} ({finishedPlayers.Count}/{NetworkServer.connections.Count})");

        // Check if all players have finished
        //int activePlayers = 0;
        //foreach (var c in NetworkServer.connections.Values)
        //    if (c != null && c.identity != null)
        //        activePlayers++;

        //if (finishedPlayers.Count >= activePlayers)
        //{
        //    // Advance to next level if available
        //    currentLevelIndex++;
        //    if (currentLevelIndex < subGameScenes.Count)
        //    {
        //        string nextScene = subGameScenes[currentLevelIndex];
        //        Debug.Log($"All players finished. Loading next level: {nextScene}");
        //        SendClientNewSceneMessage(nextScene);

        //        finishedPlayers.Clear();
        //    }
        //    else
        //    {
        //        Debug.Log("All levels complete!");
        //        // Optionally, return to lobby or end game here
        //    }
        //}

        // Advance to next level if available
        currentLevelIndex++;
        if (currentLevelIndex < subGameScenes.Count)
        {
            string nextScene = subGameScenes[currentLevelIndex];
            Debug.Log($"All players finished. Loading next level: {nextScene}");
            SendClientNewSceneMessage(nextScene);

            finishedPlayers.Clear();
        }
        else
        {
            Debug.Log("All levels complete!");
            // Optionally, return to lobby or end game here
        }
    }

    private void OnPlayerFailMessageReceived(NetworkConnectionToClient conn, PlayerFailMessage msg)
    {
        string currentScene = SceneManager.GetActiveScene().path;
        Debug.Log($"Received PlayerFinishLevelMessage from {conn.identity} in scene {currentScene}");

        if (!subGameScenes.Contains(currentScene))
            return;

        // Advance to next level if available
        Debug.Log($"Players Failed. Reset level: {currentScene}");
        SendClientNewSceneMessage(currentScene);

        finishedPlayers.Clear();
    }

    void SendClientNewSceneMessage(string sceneName)
    {
        // Sends a SceneMessage to all connected clients to load the scene
        foreach (var conn in Mirror.NetworkServer.connections.Values)
        {
            if (conn != null && conn.isReady)
            {
                conn.Send(new SceneMessage
                {
                    sceneName = sceneName,
                    sceneOperation = SceneOperation.Normal,
                    customHandling = true,
                });
            }
        }
    }

    #endregion
}
