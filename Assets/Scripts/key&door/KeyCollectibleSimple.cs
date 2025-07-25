using UnityEngine;
using UnityEngine.Events;

public class KeyCollectibleSimple : MonoBehaviour
{
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

    private Vector3 startPos;
    private bool isCollected = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Animation float
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Xoay
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    void CollectKey()
    {
        isCollected = true;

        // Effects
        if (collectEffect)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
        }

        onKeyCollected?.Invoke();

        // Destroy key
        Destroy(gameObject);
    }
}

