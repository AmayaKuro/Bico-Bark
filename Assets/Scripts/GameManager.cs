using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int score;
    /* [SerializeField] private TextMeshProUGUI PointsUI;*/
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject GameWinUI;
    private bool isDead = false;
    private bool isWin = false;

    void Start()
    {
        UpdateScore();
        GameOverUI.SetActive(false);
        GameWinUI.SetActive(false); 
    }

    // Update is called once per frame
    void Update()
    {

        
    }
    public void AddScore(int point)
    {
        score += point;
       /* UpdateScore();*/
    } 
    public void UpdateScore()
    {
       /* PointsUI.text += score.ToString();*/
    }
    public void GameOver(NetworkIdentity playerIdentity) 
    { 
        isDead = true;
        

        // Only send message if this is the local player
        if (playerIdentity && playerIdentity.isLocalPlayer)
        {
            // Send finish level message to server
            NetworkClient.connection.Send(new PlayerFailMessage {});

            // Optional: Disable further collision to prevent multiple sends
            GetComponent<BoxCollider2D>().enabled = false;
        }
            
    }
    public void GameWin()
    {
        isWin = true;
        Time.timeScale = 0;
        GameWinUI.SetActive(true);
    }
    public void restart()
    { 
        isDead=false;
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    public void Menu()
    {
        isWin = true;
        Time.timeScale=1;
        SceneManager.LoadScene("Lobby");
    }
    public bool IsGameOver()
    {
        return isDead;
    }
    public bool IsWIn()
    {
        return isWin;
    }
}
