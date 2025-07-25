using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class TilemapDisappearing : MonoBehaviour
{
    [Header("Target Tilemap")]
    public Tilemap targetTilemap; 
    public TilemapCollider2D targetCollider; 

    [Header("Timer Settings")]
    public float delayBeforeDisappear = 3f; 
    public float disappearDuration = 2f; 
    public bool disappearByTiles = true;

    [Header("Disappear Direction")]
    public DisappearDirection disappearDirection = DisappearDirection.Random;
    public enum DisappearDirection
    {
        Random,         
        LeftToRight,     
        RightToLeft,       
        TopToBottom,    
        BottomToTop,     
        FromPlayer,      
        ToPlayer         
    }
    [Header("Visual")]
    public bool showTimer = true;
    public Text timerText;
    public Color warningColor = Color.red;
    public GameObject warningEffect;

    [Header("Audio")]
    public AudioClip triggerSound; 
    public AudioClip warningSound; 
    public AudioClip disappearSound; 

    [Header("Settings")]
    public bool oneTimeOnly = true;
    public bool requireAllPlayers = false; 

    // Private
    private bool isTriggered = false;
    private bool isDisappearing = false;
    private AudioSource audioSource;
    private Color originalTilemapColor;
    private List<GameObject> playersInZone = new List<GameObject>();
    private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>();

    void Start()
    {
        // Setup
        audioSource = gameObject.AddComponent<AudioSource>();

        // Auto find tilemap if not assigned
        if (targetTilemap == null)
        {
            targetTilemap = GetComponentInChildren<Tilemap>();
        }

        if (targetTilemap != null)
        {
            originalTilemapColor = targetTilemap.color;

            // Store original tiles
            BoundsInt bounds = targetTilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = targetTilemap.GetTile(pos);
                    if (tile != null)
                    {
                        originalTiles[pos] = tile;
                    }
                }
            }
        }

        // Auto find collider
        if (targetCollider == null && targetTilemap != null)
        {
            targetCollider = targetTilemap.GetComponent<TilemapCollider2D>();
        }

        // Hide timer initially
        if (timerText) timerText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
            Debug.Log($"Player entered zone. Total: {playersInZone.Count}");

            // Check if should trigger
            if (!isTriggered && !isDisappearing)
            {
                if (!requireAllPlayers || CheckAllPlayersInZone())
                {
                    TriggerDisappear();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other.gameObject);
            Debug.Log($"Player left zone. Total: {playersInZone.Count}");
        }
    }

    bool CheckAllPlayersInZone()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        return playersInZone.Count >= allPlayers.Length;
    }

    void TriggerDisappear()
    {
        if (oneTimeOnly && isTriggered) return;

        isTriggered = true;
        Debug.Log("Zone triggered! Starting countdown...");

        // Play trigger sound
        if (triggerSound) audioSource.PlayOneShot(triggerSound);

        // Start countdown
        StartCoroutine(DisappearCountdown());
    }

    IEnumerator DisappearCountdown()
    {
        // Show timer
        if (timerText) timerText.gameObject.SetActive(true);

        float timer = delayBeforeDisappear;

        // Countdown phase
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            // Update timer display
            if (timerText)
            {
                timerText.text = Mathf.CeilToInt(timer).ToString();

                // Flash when low time
                if (timer <= 3)
                {
                    timerText.color = Color.Lerp(Color.white, warningColor, Mathf.PingPong(Time.time * 4, 1));
                }
            }

            // Warning effects at 3 seconds
            if (timer <= 3 && timer > 2.9f)
            {
                if (warningSound) audioSource.PlayOneShot(warningSound);
                if (warningEffect) Instantiate(warningEffect, targetTilemap.transform.position, Quaternion.identity);
            }

            // Flash tilemap
            if (timer <= 3 && targetTilemap)
            {
                float flash = Mathf.PingPong(Time.time * 3, 1);
                targetTilemap.color = Color.Lerp(originalTilemapColor, warningColor, flash * 0.5f);
            }

            yield return null;
        }

        // Hide timer
        if (timerText) timerText.gameObject.SetActive(false);

        // Start disappearing
        isDisappearing = true;
        if (disappearSound) audioSource.PlayOneShot(disappearSound);

        if (disappearByTiles)
        {
            yield return StartCoroutine(DisappearByTiles());
        }
        else
        {
            yield return StartCoroutine(DisappearWholeTilemap());
        }

        isDisappearing = false;
    }

    IEnumerator DisappearByTiles()
    {
        List<Vector3Int> tilePositions = new List<Vector3Int>(originalTiles.Keys);

        switch (disappearDirection)
        {
            case DisappearDirection.LeftToRight:
                tilePositions.Sort((a, b) => a.x.CompareTo(b.x));
                break;

            case DisappearDirection.RightToLeft:
                tilePositions.Sort((a, b) => b.x.CompareTo(a.x));
                break;

            case DisappearDirection.TopToBottom:
                tilePositions.Sort((a, b) => b.y.CompareTo(a.y));
                break;

            case DisappearDirection.BottomToTop:
                tilePositions.Sort((a, b) => a.y.CompareTo(b.y));
                break;

            case DisappearDirection.FromPlayer:
                GameObject player = playersInZone.Count > 0 ? playersInZone[0] : GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector3 playerPos = targetTilemap.WorldToCell(player.transform.position);
                    tilePositions.Sort((a, b) =>
                    {
                        float distA = Vector3Int.Distance(a, Vector3Int.FloorToInt(playerPos));
                        float distB = Vector3Int.Distance(b, Vector3Int.FloorToInt(playerPos));
                        return distA.CompareTo(distB); 
                    });
                }
                break;

            case DisappearDirection.ToPlayer:
                GameObject player2 = playersInZone.Count > 0 ? playersInZone[0] : GameObject.FindWithTag("Player");
                if (player2 != null)
                {
                    Vector3 playerPos = targetTilemap.WorldToCell(player2.transform.position);
                    tilePositions.Sort((a, b) =>
                    {
                        float distA = Vector3Int.Distance(a, Vector3Int.FloorToInt(playerPos));
                        float distB = Vector3Int.Distance(b, Vector3Int.FloorToInt(playerPos));
                        return distB.CompareTo(distA); 
                    });
                }
                break;

            default: // Random
                for (int i = 0; i < tilePositions.Count; i++)
                {
                    Vector3Int temp = tilePositions[i];
                    int randomIndex = Random.Range(i, tilePositions.Count);
                    tilePositions[i] = tilePositions[randomIndex];
                    tilePositions[randomIndex] = temp;
                }
                break;
        }

        float delayPerTile = disappearDuration / tilePositions.Count;

        foreach (var pos in tilePositions)
        {
            // Check pause
            while (DisappearingManager.IsPaused)
            {
                if (targetTilemap)
                    targetTilemap.color = originalTilemapColor;
                yield return null;
            }

            if (targetTilemap.GetTile(pos) != null)
            {
                // Optional: Flash effect
                if (warningEffect)
                {
                    Vector3 worldPos = targetTilemap.CellToWorld(pos) + targetTilemap.cellSize / 2;
                    GameObject effect = Instantiate(warningEffect, worldPos, Quaternion.identity);
                    effect.transform.localScale = Vector3.one * 0.5f;
                }

                // Remove tile
                targetTilemap.SetTile(pos, null);

                // Update collider
                if (targetCollider)
                {
                    targetCollider.ProcessTilemapChanges();
                }

                yield return new WaitForSeconds(delayPerTile);
            }
        }

        if (targetCollider) targetCollider.enabled = false;
    }

    IEnumerator DisappearWholeTilemap()
    {
        float elapsed = 0f;

        // Fade out effect
        while (elapsed < disappearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / disappearDuration;

            // Fade tilemap
            if (targetTilemap)
            {
                float alpha = Mathf.Lerp(1f, 0f, t);
                targetTilemap.color = new Color(originalTilemapColor.r, originalTilemapColor.g, originalTilemapColor.b, alpha);
            }

            yield return null;
        }

        // Disable everything
        if (targetTilemap) targetTilemap.gameObject.SetActive(false);
        if (targetCollider) targetCollider.enabled = false;
    }

    // Public methods
    public void ResetTilemap()
    {
        if (targetTilemap == null) return;

        // Restore all tiles
        foreach (var kvp in originalTiles)
        {
            targetTilemap.SetTile(kvp.Key, kvp.Value);
        }

        // Reset visual
        targetTilemap.color = originalTilemapColor;
        targetTilemap.gameObject.SetActive(true);

        // Reset collider
        if (targetCollider)
        {
            targetCollider.enabled = true;
            targetCollider.ProcessTilemapChanges();
        }

        // Reset states
        isTriggered = false;
        isDisappearing = false;
        playersInZone.Clear();
    }

    // Gizmos
    void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (!box) return;

        // Zone color
        Color zoneColor = isTriggered ? warningColor : new Color(0, 1, 0, 0.3f);

        // Draw zone
        Gizmos.color = zoneColor;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.DrawWireCube(box.offset, box.size);
        Gizmos.matrix = oldMatrix;

        // Draw connection to tilemap
        if (targetTilemap != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetTilemap.transform.position);
        }

#if UNITY_EDITOR
        // Label
        string status = isTriggered ? "TRIGGERED" : "READY";
        UnityEditor.Handles.Label(transform.position + Vector3.up,
            $"Tilemap Trigger Zone\n[{status}]\nDelay: {delayBeforeDisappear}s");
#endif
    }
}