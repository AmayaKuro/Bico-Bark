using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class KeyManager : MonoBehaviour
{
    [Header("Key Management")]
    public List<string> allKeyIDs = new List<string>();
    public int totalKeys = 0;
    public int collectedKeys = 0;

    [Header("UI")]
    public GameObject keyUIContainer;
    public GameObject keyUIPrefab;
    public TMPro.TextMeshProUGUI keyCountText;
    public GameObject allKeysCollectedUI;

    [Header("Events")]
    public UnityEvent onKeyCollected;
    public UnityEvent onAllKeysCollected;

    private Dictionary<string, bool> keyStates = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> keyUIElements = new Dictionary<string, GameObject>();

    void Start()
    {
        InitializeKeys();
        UpdateUI();
    }

    [System.Obsolete]
    void InitializeKeys()
    {
        // Find all keys in scene if list is empty
        if (allKeyIDs.Count == 0)
        {
            KeyCollectible[] keys = FindObjectsOfType<KeyCollectible>();
            foreach (var key in keys)
            {
                allKeyIDs.Add(key.keyID);
            }
        }

        totalKeys = allKeyIDs.Count;

        // Check saved states
        foreach (string keyID in allKeyIDs)
        {
            bool collected = PlayerPrefs.GetInt($"Key_{keyID}_Collected", 0) == 1;
            keyStates[keyID] = collected;
            if (collected) collectedKeys++;

            // Create UI element
            if (keyUIPrefab && keyUIContainer)
            {
                GameObject keyUI = Instantiate(keyUIPrefab, keyUIContainer.transform);
                keyUI.name = $"KeyUI_{keyID}";
                keyUIElements[keyID] = keyUI;

                // Set initial state
                SetKeyUIState(keyUI, collected);
            }
        }
    }

    public void CollectKey(string keyID)
    {
        if (!keyStates.ContainsKey(keyID)) return;

        if (!keyStates[keyID])
        {
            keyStates[keyID] = true;
            collectedKeys++;

            // Update UI
            if (keyUIElements.ContainsKey(keyID))
            {
                SetKeyUIState(keyUIElements[keyID], true);
            }

            UpdateUI();

            // Events
            onKeyCollected?.Invoke();

            if (collectedKeys >= totalKeys)
            {
                onAllKeysCollected?.Invoke();
                if (allKeysCollectedUI)
                {
                    allKeysCollectedUI.SetActive(true);
                }
            }
        }
    }

    void SetKeyUIState(GameObject keyUI, bool collected)
    {
        // Change color or sprite
        Image img = keyUI.GetComponent<Image>();
        if (img)
        {
            img.color = collected ? Color.white : new Color(1, 1, 1, 0.3f);
        }

        // Add checkmark or effect
        Transform checkmark = keyUI.transform.Find("Checkmark");
        if (checkmark)
        {
            checkmark.gameObject.SetActive(collected);
        }
    }

    void UpdateUI()
    {
        if (keyCountText)
        {
            keyCountText.text = $"{collectedKeys}/{totalKeys}";
        }
    }

    public bool HasKey(string keyID)
    {
        return keyStates.ContainsKey(keyID) && keyStates[keyID];
    }

    public bool HasAllKeys()
    {
        return collectedKeys >= totalKeys;
    }

    public void ShowRequiredKeys(List<string> requiredKeys)
    {
        // Flash or highlight required keys in UI
        foreach (string keyID in requiredKeys)
        {
            if (!HasKey(keyID) && keyUIElements.ContainsKey(keyID))
            {
                StartCoroutine(FlashKeyUI(keyUIElements[keyID]));
            }
        }
    }

    IEnumerator FlashKeyUI(GameObject keyUI)
    {
        Image img = keyUI.GetComponent<Image>();
        if (!img) yield break;

        Color original = img.color;

        for (int i = 0; i < 3; i++)
        {
            img.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            img.color = original;
            yield return new WaitForSeconds(0.2f);
        }
    }

    [ContextMenu("Reset All Keys")]
    public void ResetAllKeys()
    {
        foreach (string keyID in allKeyIDs)
        {
            PlayerPrefs.DeleteKey($"Key_{keyID}_Collected");
        }
        PlayerPrefs.Save();

        // Reset scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

// ===== 4. KEY UI ELEMENT =====
public class KeyUIElement : MonoBehaviour
{
    public Image keyIcon;
    public Image checkmark;
    public TMPro.TextMeshProUGUI keyLabel;
    public Animator animator;

    public void SetKeyData(string keyID, Sprite keySprite = null)
    {
        if (keyLabel)
            keyLabel.text = keyID;

        if (keySprite && keyIcon)
            keyIcon.sprite = keySprite;
    }

    public void SetCollected(bool collected)
    {
        if (checkmark)
            checkmark.gameObject.SetActive(collected);

        if (animator)
            animator.SetBool("Collected", collected);

        if (keyIcon)
            keyIcon.color = collected ? Color.white : new Color(1, 1, 1, 0.5f);
    }
}

