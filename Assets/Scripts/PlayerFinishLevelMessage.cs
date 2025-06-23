using Mirror;
using UnityEngine;

public struct PlayerFinishLevelMessage : NetworkMessage
{
    public GameObject player; // or any data you want to send
}
