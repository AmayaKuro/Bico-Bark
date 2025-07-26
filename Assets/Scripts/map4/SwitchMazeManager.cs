using Mirror;
using Mirror.Examples.Common.Controllers.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class SwitchMazeManager : NetworkBehaviour
{
    [Header("Maze Configuration")]
    public List<MazeSwitch> allSwitches = new List<MazeSwitch>();
    public List<MazeDoor> allDoors = new List<MazeDoor>();
    public Transform goalPosition;

    [Header("Timer")]
    public bool useTimer = false;
    public float timeLimit = 120f; // 2 minutes
    public TextMeshProUGUI timerText;
    [Header("Victory")]
    public GameObject victoryEffect;
    public UnityEvent onMazeComplete;

    private float currentTime;
    private bool isComplete = false;

    [System.Obsolete]
    void Start()
    {
        // Auto-find all switches and doors if not assigned
        if (allSwitches.Count == 0)
            allSwitches.AddRange(FindObjectsOfType<MazeSwitch>());

        if (allDoors.Count == 0)
            allDoors.AddRange(FindObjectsOfType<MazeDoor>());

        currentTime = timeLimit;

        // Log initial state
        LogMazeState();
    }

    void Update()
    {
        if (useTimer && !isComplete)
        {
            currentTime -= Time.deltaTime;

            if (timerText)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            if (currentTime <= 0)
            {
                OnTimeOut();
            }
        }

        // Debug reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetMaze();
        }
    }

    public void CheckVictory()
    {
        // Check if goal door is open
        MazeDoor goalDoor = allDoors.Find(d => d.doorID == "D6");
        if (goalDoor && goalDoor.IsOpen())
        {
            OnMazeComplete();
        }
    }

    void OnMazeComplete()
    {
        if (isComplete) return;

        isComplete = true;
        Debug.Log("MAZE COMPLETE!");

        if (victoryEffect && goalPosition)
        {
            Instantiate(victoryEffect, goalPosition.position, Quaternion.identity);
        }

        onMazeComplete?.Invoke();
    }

    void OnTimeOut()
    {
        Debug.Log("GAME OVER - Time Out!");

    

        if (timerText)
        {
            timerText.text = "GAME OVER";
            timerText.color = Color.red;
            timerText.fontSize = 48;
        }
        connectionToServer.Send(new PlayerFailMessage { });
        enabled = false;

        StartCoroutine(ReloadAfterDelay());
    }
    IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ResetMaze()
    {
        // Reset all switches
        foreach (var switch_ in allSwitches)
        {
            switch_.ResetSwitch();
        }

        // Timer
        currentTime = timeLimit;
        isComplete = false;

        Debug.Log("Maze reset!");
    }

    void LogMazeState()
    {
        Debug.Log("=== MAZE STATE ===");
        foreach (var switch_ in allSwitches)
        {
            Debug.Log($"Switch {switch_.switchID}: {switch_.GetState()}");
        }
        foreach (var door in allDoors)
        {
            Debug.Log($"Door {door.doorID}: {(door.IsOpen() ? "OPEN" : "CLOSED")}");
        }
    }

    // Editor helper
    [ContextMenu("Connect All Switches")]
    void ConnectAllSwitchesInEditor()
    {
        // Auto-connect based on naming convention
        foreach (var switch_ in allSwitches)
        {
            switch_.connectedDoors.Clear();

            // Find doors that should connect to this switch
            foreach (var door in allDoors)
            {
                // Example: Switch A connects to doors containing "A" in their ID
                if (ShouldConnect(switch_.switchID, door.doorID))
                {
                    switch_.connectedDoors.Add(door);
                }
            }
        }

        Debug.Log("Auto-connected switches to doors!");
    }

    bool ShouldConnect(string switchID, string doorID)
    {
        // Define your connection logic here
        // Example: Based on the maze design
        switch (switchID)
        {
            case "A": return doorID == "D1" || doorID == "D3" || doorID == "D5";
            case "B": return doorID == "D2" || doorID == "D5";
            case "C": return doorID == "D1" || doorID == "D2" || doorID == "D4";
            case "D": return doorID == "D3" || doorID == "D4" || doorID == "D6";
            default: return false;
        }
    }
    public void AddTime(float seconds)
    {
        if (useTimer && !isComplete)
        {
            currentTime += seconds;
            Debug.Log($"Time bonus: +{seconds}s! Total: {currentTime}s");

            // Flash timer UI
            if (timerText)
                StartCoroutine(FlashTimer());
        }
    }

    IEnumerator FlashTimer()
    {
        Color original = timerText.color;
        timerText.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        timerText.color = original;
    }
}