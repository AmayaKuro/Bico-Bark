using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class SimpleExitDoor : NetworkBehaviour
{
    [Header("Door Settings")]
    public GameObject doorObject;

    [Header("Effects")]
    public GameObject appearEffect;
    public AudioClip appearSound;

    [Header("Optional Animation")]
    public bool useAnimation = false;
    public float animationDuration = 1f;

    private bool isDoorOpen = false;

    void Start()
    {
        if (doorObject)
        {
            doorObject.SetActive(false);
        }
    }

    // Called from KeyCollectible when key is collected
    public void OnKeyCollected()
    {
        ShowDoor();
    }

    void ShowDoor()
    {
        if (!doorObject || isDoorOpen) return;

        isDoorOpen = true;

        // Effects
        if (appearEffect)
        {
            Instantiate(appearEffect, doorObject.transform.position, Quaternion.identity);
        }

        if (appearSound)
        {
            AudioSource.PlayClipAtPoint(appearSound, doorObject.transform.position);
        }

        // Show door
        if (useAnimation)
        {
            StartCoroutine(AnimateDoorAppear());
        }
        else
        {
            doorObject.SetActive(true);
        }
    }

    void OnTriggerEnter2D(BoxCollider2D other)
    {
        // Only process if door is open
        if (!isDoorOpen) return;

        // Check if it's a player
        if (other.CompareTag("Player"))
        {
            
                // Send finish level message to server
                connectionToServer.Send(new PlayerFinishLevelMessage { player = this.GameObject() });

                // Optional: Disable further collision to prevent multiple sends
                GetComponent<Collider2D>().enabled = false;
            
        }
    }

    IEnumerator AnimateDoorAppear()
    {
        doorObject.SetActive(true);
        Vector3 originalScale = doorObject.transform.localScale;
        doorObject.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            doorObject.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        doorObject.transform.localScale = originalScale;
    }
}