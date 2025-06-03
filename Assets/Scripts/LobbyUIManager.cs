using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject playerSlotPrefab;
    public Transform playerListContent;
    public Button startGameButton;

    private List<GameObject> currentSlots = new List<GameObject>();

    void Start()
    {
        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    public void UpdatePlayerList(List<string> playerNames)
    {
        // Clear existing
        foreach (var slot in currentSlots)
            Destroy(slot);
        currentSlots.Clear();

        // Populate new
        foreach (string name in playerNames)
        {
            GameObject slot = Instantiate(playerSlotPrefab, playerListContent);
            var tmpText = slot.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = name;
            }
            currentSlots.Add(slot);
        }

        statusText.text = $"Players: {playerNames.Count}/8";
    }

    void OnStartGameClicked()
    {
        Debug.Log("Start Game clicked");
        // Call start game logic (via NetworkManager)
    }
}
