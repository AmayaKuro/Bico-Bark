using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MazeSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    public string switchID = "A";
    public Color switchColor = Color.red;
    public bool isToggle = true; // true = toggle, false = hold
    public bool startState = false; // false = OFF

    [Header("Connected Doors")]
    public List<MazeDoor> connectedDoors = new List<MazeDoor>();

    [Header("Visual")]
    public SpriteRenderer switchVisual;
    public GameObject activateEffect;
    public Color offColor = Color.gray;
    public Color onColor = Color.green;

    [Header("Audio")]
    public AudioClip switchSound;

    [Header("Events")]
    public UnityEvent onSwitchOn;
    public UnityEvent onSwitchOff;

    // Private
    private bool currentState;
    private int playersOnSwitch = 0;
    private AudioSource audioSource;

    void Start()
    {
        currentState = startState;
        audioSource = gameObject.AddComponent<AudioSource>();

        // Setup visual
        if (!switchVisual) switchVisual = GetComponent<SpriteRenderer>();
        UpdateVisual();

        // Set initial door states
        UpdateConnectedDoors();

        // Draw connection lines
        DrawConnections();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersOnSwitch++;

            if (playersOnSwitch == 1) // First player
            {
                if (isToggle)
                {
                    // Toggle mode
                    ToggleSwitch();
                }
                else
                {
                    // Hold mode - turn ON
                    SetSwitchState(true);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersOnSwitch--;

            if (playersOnSwitch == 0 && !isToggle)
            {
                // Hold mode - turn OFF when no players
                SetSwitchState(false);
            }
        }
    }

    void ToggleSwitch()
    {
        SetSwitchState(!currentState);
    }

    void SetSwitchState(bool newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Effects
        if (switchSound) audioSource.PlayOneShot(switchSound);
        if (activateEffect) Instantiate(activateEffect, transform.position, Quaternion.identity);

        // Update doors
        UpdateConnectedDoors();

        // Visual
        UpdateVisual();

        // Events
        if (currentState)
            onSwitchOn?.Invoke();
        else
            onSwitchOff?.Invoke();

        Debug.Log($"Switch {switchID}: {(currentState ? "ON" : "OFF")}");
    }

    void UpdateConnectedDoors()
    {
        foreach (var door in connectedDoors)
        {
            if (door != null)
            {
                door.ToggleDoorState();
            }
        }
    }

    void UpdateVisual()
    {
        if (switchVisual)
        {
            switchVisual.color = currentState ? switchColor : offColor;

            // Scale effect
            float scale = currentState ? 1.1f : 1f;
            transform.localScale = Vector3.one * scale;
        }
    }

    void DrawConnections()
    {
        // Create line renderers to doors
        foreach (var door in connectedDoors)
        {
            if (door != null)
            {
                GameObject lineObj = new GameObject($"Line_{switchID}_to_{door.doorID}");
                lineObj.transform.parent = transform;

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = new Color(switchColor.r, switchColor.g, switchColor.b, 0.3f);
                line.endColor = new Color(switchColor.r, switchColor.g, switchColor.b, 0.1f);

                line.SetPosition(0, transform.position);
                line.SetPosition(1, door.transform.position);

                line.enabled = false; // Hide by default, show in editor
            }
        }
    }

    // Public methods
    public void ResetSwitch()
    {
        SetSwitchState(startState);
    }

    public bool GetState()
    {
        return currentState;
    }

    // Editor visualization
    void OnDrawGizmos()
    {
        // Draw switch
        Gizmos.color = switchColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one);

        // Draw connections
        if (connectedDoors != null)
        {
            foreach (var door in connectedDoors)
            {
                if (door != null)
                {
                    Gizmos.color = new Color(switchColor.r, switchColor.g, switchColor.b, 0.5f);
                    Gizmos.DrawLine(transform.position, door.transform.position);
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up,
            $"Switch {switchID}\n{(connectedDoors != null ? connectedDoors.Count : 0)} doors");
#endif
    }
}