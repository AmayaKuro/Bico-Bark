using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitionZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public int requiredPlayers = 2; 
    public bool requireAllPlayers = false; 
    public float activationDelay = 0.5f; 

    [Header("Camera Settings")]
    public Transform newCameraPosition; 
    public float transitionDuration = 2f; 
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Transition Options")]
    public TransitionType transitionType = TransitionType.Smooth;
    public enum TransitionType
    {
        Smooth,     
        Instant,    
        CutOnBlack  
    }

    [Header("?? Advanced")]
    public bool lockPlayersOnTransition = true; 
    public bool oneTimeOnly = true; 
    public GameObject[] objectsToActivate; 
    public GameObject[] objectsToDeactivate; 

    [Header("?? Visual Feedback")]
    public SpriteRenderer zoneVisual;
    public Color inactiveColor = new Color(1, 0, 0, 0.3f);
    public Color readyColor = new Color(0, 1, 0, 0.3f);
    public GameObject activationEffect;

    [Header("?? Audio")]
    public AudioClip transitionSound;

    // Private variables
    private Camera mainCamera;
    private List<GameObject> playersInZone = new List<GameObject>();
    private bool isActivated = false;
    private bool isTransitioning = false;
    private float activationTimer = 0f;
    private int totalPlayerCount;
    private AudioSource audioSource;

    // For smooth transition
    private Vector3 originalCameraPos;
    private Vector3 originalCameraRot;

    void Start()
    {
        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            enabled = false;
            return;
        }

        // Get total player count
        UpdateTotalPlayerCount();

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();

        // Setup visual
        if (zoneVisual)
        {
            zoneVisual.color = inactiveColor;
        }

        // Create visual position marker if not exists
        if (newCameraPosition == null)
        {
            GameObject marker = new GameObject("New Camera Position");
            marker.transform.position = transform.position + Vector3.up * 5f;
            newCameraPosition = marker.transform;
        }
    }

    void UpdateTotalPlayerCount()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        totalPlayerCount = players.Length;

        // Update required players if using "all players" mode
        if (requireAllPlayers)
        {
            requiredPlayers = totalPlayerCount;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
            OnPlayerEnter();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other.gameObject);
            OnPlayerExit();
        }
    }

    void OnPlayerEnter()
    {
        Debug.Log($"Players in zone: {playersInZone.Count}/{requiredPlayers}");

        // Update visual
        UpdateZoneVisual();

        // Check if we have enough players
        if (playersInZone.Count >= requiredPlayers && !isActivated && !isTransitioning)
        {
            activationTimer = 0f;
        }
    }

    void OnPlayerExit()
    {
        Debug.Log($"Player left. In zone: {playersInZone.Count}/{requiredPlayers}");

        // Reset timer
        activationTimer = 0f;

        // Update visual
        UpdateZoneVisual();
    }

    void Update()
    {
        // Check for activation
        if (!isActivated && !isTransitioning && playersInZone.Count >= requiredPlayers)
        {
            activationTimer += Time.deltaTime;

            // Visual feedback for timer
            if (zoneVisual && activationDelay > 0)
            {
                float progress = activationTimer / activationDelay;
                Color currentColor = Color.Lerp(inactiveColor, readyColor, progress);
                zoneVisual.color = currentColor;
            }

            if (activationTimer >= activationDelay)
            {
                ActivateTransition();
            }
        }
    }

    void ActivateTransition()
    {
        if (oneTimeOnly && isActivated) return;

        isActivated = true;
        isTransitioning = true;

        // Sound
        if (transitionSound && audioSource)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // Effect
        if (activationEffect)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }

        // Lock players if needed
        if (lockPlayersOnTransition)
        {
            SetPlayersMovement(false);
        }

        // Start transition
        switch (transitionType)
        {
            case TransitionType.Smooth:
                StartCoroutine(SmoothTransition());
                break;
            case TransitionType.Instant:
                InstantTransition();
                break;
            case TransitionType.CutOnBlack:
                StartCoroutine(FadeTransition());
                break;
        }
    }
    
    IEnumerator SmoothTransition()
    {
        originalCameraPos = mainCamera.transform.position;
        originalCameraRot = mainCamera.transform.eulerAngles;

        Vector3 targetPos = new Vector3(
            newCameraPosition.position.x,
            newCameraPosition.position.y,
            mainCamera.transform.position.z // Keep camera Z
        );

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveT = transitionCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(originalCameraPos, targetPos, curveT);

            yield return null;
        }

        mainCamera.transform.position = targetPos;
        OnTransitionComplete();
    }

    void InstantTransition()
    {
        Vector3 targetPos = new Vector3(
            newCameraPosition.position.x,
            newCameraPosition.position.y,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = targetPos;
        OnTransitionComplete();
    }

    IEnumerator FadeTransition()
    {
        // Would need a UI panel for fade effect
        // For now, just do instant
        yield return new WaitForSeconds(0.5f);
        InstantTransition();
    }

    void OnTransitionComplete()
    {
        isTransitioning = false;

        // Activate/Deactivate objects
        foreach (var obj in objectsToActivate)
        {
            if (obj) obj.SetActive(true);
        }

        foreach (var obj in objectsToDeactivate)
        {
            if (obj) obj.SetActive(false);
        }

        // Unlock players
        if (lockPlayersOnTransition)
        {
            SetPlayersMovement(true);
        }

        // Update visual
        if (zoneVisual)
        {
            zoneVisual.enabled = false;
        }

        Debug.Log("Camera transition complete!");
    }

    void SetPlayersMovement(bool canMove)
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in allPlayers)
        {
            var controller = player.GetComponent<PlayerControl>();
            if (controller)
            {
                controller.enabled = canMove;
            }

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb && !canMove)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void UpdateZoneVisual()
    {
        if (zoneVisual)
        {
            float fillPercent = (float)playersInZone.Count / requiredPlayers;
            Color targetColor = Color.Lerp(inactiveColor, readyColor, fillPercent);
            zoneVisual.color = targetColor;
        }
    }

    // Gizmos for editor
    void OnDrawGizmos()
    {
        // Draw zone
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box)
        {
            Gizmos.DrawCube(transform.position, box.size);
        }

        // Draw camera positions
        if (newCameraPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(newCameraPosition.position, Vector3.one * 2);
            Gizmos.DrawLine(transform.position, newCameraPosition.position);

            // Draw camera view preview
            float height = Camera.main ? Camera.main.orthographicSize * 2 : 10;
            float width = height * 16f / 9f; // Assume 16:9
            Gizmos.DrawWireCube(newCameraPosition.position, new Vector3(width, height, 0));
        }

        // Label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up,
            $"Camera Zone\nNeed {requiredPlayers} players");
#endif
    }
}
