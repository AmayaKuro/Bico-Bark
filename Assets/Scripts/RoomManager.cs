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

    // This is set true after server loads all subscene instances
    bool subscenesLoaded;

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
        base.OnRoomServerSceneChanged(sceneName);

        // Only subscribe in game scenes, not in the room scene
        if (subGameScenes.Contains(sceneName))
        {
            // Register the handler for PlayerFinishLevelMessage
            NetworkServer.RegisterHandler<PlayerFinishLevelMessage>(OnPlayerFinishLevelMessageReceived, false);
            Debug.Log("Subscribed to PlayerFinishLevelMessage in scene: " + sceneName);
        }
    }

    private IEnumerator WaitForSceneLoad(string sceneName)
    {
        // Wait for the scene to load
        yield return new WaitForSeconds(5);
        // Ensure all clients are ready before changing the scene
        foreach (var conn in Mirror.NetworkServer.connections.Values)
        {
            if (conn != null && !conn.isReady)
            {
                Mirror.NetworkServer.SetClientReady(conn);
            }
        }
        // After the scene is loaded and clients are ready, change the scene
        ServerChangeScene("Game " + Random.Range(1, 4));
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
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {

        // Calculate X position based on the number of players already in the game
        //int playerIndex = NetworkServer.connections.Count - 1; // 0-based index
        //float spacing = 2.0f; // Adjust as needed for your player size

        //// Set the spawn position: expand in X, keep Y and Z from the start position
        //Vector3 basePosition = NetworkManager.startPositions.Count > 0    
        //    ? NetworkManager.startPositions[0].position
        //    : Vector3.zero;
        //Vector3 spawnPosition = basePosition + new Vector3(playerIndex * spacing, 0, 0);
        DontDestroyOnLoad(gamePlayer); // Ensure the game player persists across scene 

        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        // Choose a sub-game scene to load. For example, pick one at random:
        if (subGameScenes != null && subGameScenes.Count > 0)
        {
            string selectedScene = subGameScenes[currentLevelIndex];
            ServerChangeScene(selectedScene);

            Debug.Log($"Loading sub-game scene: {selectedScene}");
        }
        else
        {
            // Fallback to default behavior if no sub-game scenes are set
            base.OnRoomServerPlayersReady();
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Only add players in the room scene
        if (Utils.IsSceneActive(RoomScene))
        {
            allPlayersReady = false;

            GameObject newRoomGameObject = OnRoomServerCreateRoomPlayer(conn);
            if (newRoomGameObject == null)
                newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
        }
        if (subGameScenes.Contains(SceneManager.GetActiveScene().path))
        {
            allPlayersReady = false;

            var currentPlayerObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            NetworkServer.ReplacePlayerForConnection(conn, currentPlayerObject, ReplacePlayerOptions.KeepAuthority);
        }
        else
        {
            // Do NOT disconnect the player if not in the room scene.
            // Optionally, you can log or handle late join attempts here.
            Debug.Log($"OnServerAddPlayer called outside RoomScene for {conn}, ignoring.");
            // Do nothing, don't disconnect.
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
        foreach (var subScene in subGameScenes)
        {
            Debug.Log($"Sub-game scene: {subScene}");
        }
        if (!subGameScenes.Contains(currentScene))
            return;

        // Track by NetworkIdentity netId for uniqueness
        uint netId = conn.identity != null ? conn.identity.netId : 0;
        if (netId == 0)
            return;

        finishedPlayers.Add(netId);

        Debug.Log($"Player {conn.identity} finished the level in scene {currentScene} ({finishedPlayers.Count}/{NetworkServer.connections.Count})");

        // Check if all players have finished
        int activePlayers = 0;
        foreach (var c in NetworkServer.connections.Values)
            if (c != null && c.identity != null)
                activePlayers++;

        if (finishedPlayers.Count >= activePlayers)
        {
            // Advance to next level if available
            currentLevelIndex++;
            if (currentLevelIndex < subGameScenes.Count)
            {
                string nextScene = subGameScenes[currentLevelIndex];
                Debug.Log($"All players finished. Loading next level: {nextScene}");
                finishedPlayers.Clear();
                ServerChangeScene(nextScene);
            }
            else
            {
                Debug.Log("All levels complete!");
                // Optionally, return to lobby or end game here
            }
        }
    }

    #endregion
}
