using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Map Elements")]
    public ButtonController upperButton;    // Button above
    public ButtonController lowerButton;    // Button below
    public GameObject apple;               // apple object
    public Transform spawnPoint;           // spawn players

    [Header("Players")]
    public GameObject[] playerPrefabs;
    private List<GameObject> spawnedPlayers = new List<GameObject>();

    void Start()
    {
        SpawnPlayers();
        SetupButtonConnections();
    }

    void SpawnPlayers()
    {
        // Spawn players at the spawn point
        foreach (var prefab in playerPrefabs)
        {
            GameObject player = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            spawnedPlayers.Add(player);
        }
    }

    void SetupButtonConnections()
    {
        // Setup visual connections
        if (upperButton && upperButton.connectionLine)
        {
            upperButton.connectionLine.startColor = Color.blue;
        }

        if (lowerButton && lowerButton.connectionLine)
        {
            lowerButton.connectionLine.startColor = Color.red;
        }
    }
}