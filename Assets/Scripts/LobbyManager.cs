using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkManager
{
    [Header("UI Manager")]
    public LobbyUIManager lobbyUIManager;
    public Dictionary<NetworkConnectionToClient, NetworkIdentity> connectedPlayers = new Dictionary<NetworkConnectionToClient, NetworkIdentity>();

    public override void Start()
    {
        base.Start();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn); // This spawns the player prefab and assigns conn.identity

        if (conn.identity != null)
        {
            connectedPlayers[conn] = conn.identity;
        }

        UpdateLobbyUI();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        connectedPlayers.Remove(conn);
        base.OnServerDisconnect(conn);

        UpdateLobbyUI();
    }

    public void StartGame()
    {
        if (connectedPlayers.Count >= 4)
        {
            Debug.Log("Starting game with " + connectedPlayers.Count + " players.");
            ServerChangeScene("Game");
        }
    }

    void UpdateLobbyUI()
    {
        if (lobbyUIManager == null) return;

        List<string> names = new();
        foreach (var player in connectedPlayers)
        {
            player.Value.TryGetComponent<PlayerControl>(out var playerControl);
            names.Add(playerControl.playerName);
        }

        lobbyUIManager.UpdatePlayerList(names);
    }
}
