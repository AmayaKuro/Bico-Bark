using UnityEngine;
using UnityEngine.Events;

public class KeyCollectible : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Key Settings")]
    public string keyID = "Key_01";
    public bool destroyOnCollect = true;
    public bool saveState = true;

    [Header("Visual")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.3f;
    public float rotateSpeed = 90f;

    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Events")]
    public UnityEvent onKeyCollected;
    public UnityEvent<string> onKeyCollectedWithID;

    private Vector3 startPos;
    private bool isCollected = false;

    void Start()
    {
        startPos = transform.position;

        // Check if already collected
        if (saveState && PlayerPrefs.GetInt($"Key_{keyID}_Collected", 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Float animation
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotate
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    [System.Obsolete]
    void CollectKey()
    {
        isCollected = true;

        // Save state
        if (saveState)
        {
            PlayerPrefs.SetInt($"Key_{keyID}_Collected", 1);
            PlayerPrefs.Save();
        }

        // Effects
        if (collectEffect)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
        }

        // Events
        onKeyCollected?.Invoke();
        onKeyCollectedWithID?.Invoke(keyID);

        // Update KeyManager
        KeyManager keyManager = FindObjectOfType<KeyManager>();
        if (keyManager)
        {
            keyManager.CollectKey(keyID);
        }

        // Destroy or deactivate
        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    // Reset key (for testing)
    [ContextMenu("Reset Key")]
    public void ResetKey()
    {
        PlayerPrefs.DeleteKey($"Key_{keyID}_Collected");
        isCollected = false;
        gameObject.SetActive(true);
    }
}
