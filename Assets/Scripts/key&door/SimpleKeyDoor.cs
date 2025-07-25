using UnityEngine;

public class SimpleKeyDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject door; 
    public string requiredKeyID = "Key_01";

    [Header("Optional Effects")]
    public GameObject appearEffect;
    public AudioClip appearSound;

    void Start()
    {
        bool hasKey = PlayerPrefs.GetInt($"Key_{requiredKeyID}_Collected", 0) == 1;

        door.SetActive(hasKey);
    }

    public void OnKeyCollected(string keyID)
    {
        if (keyID == requiredKeyID)
        {
            ShowDoor();
        }
    }

    void ShowDoor()
    {
        // Effects
        if (appearEffect && door.transform)
        {
            Instantiate(appearEffect, door.transform.position, Quaternion.identity);
        }

        if (appearSound)
        {
            AudioSource.PlayClipAtPoint(appearSound, door.transform.position);
        }

        door.SetActive(true);
    }

    [ContextMenu("Reset Door")]
    public void ResetDoor()
    {
        PlayerPrefs.DeleteKey($"Key_{requiredKeyID}_Collected");
        door.SetActive(false);
    }
}