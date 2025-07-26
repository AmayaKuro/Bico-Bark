using UnityEngine;

public class FinalScene : MonoBehaviour
{   
    public RoomManager roomManager; // Assign via Inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomManager = FindObjectOfType<RoomManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnExitButtonClicked()
    {
        roomManager.StopHost();
        roomManager.StopClient();
        roomManager.StopServer();
    }
}
