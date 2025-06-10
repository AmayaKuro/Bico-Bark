using UnityEditor.Build.Content;
using UnityEngine;
using Mirror;

public class PlayerCollision : NetworkBehaviour
{
    private GameManager gameManager;
    private void Awake()
    {
            gameManager= FindAnyObjectByType<GameManager>();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object has a GameManager component
        /* GameManager gameManager = collision.GetComponent<GameManager>();
         if (gameManager != null)
         {
             // If it does, set the game variable to that instance
             game = gameManager;
             Debug.Log("GameManager found and assigned.");
         }
         else
         {
             Debug.LogWarning("No GameManager found in the collided object.");
         }*/
        if (collision.CompareTag("End"))
        {
            gameManager.GameWin();
            
            
        }else if (collision.CompareTag("Traps"))
        {
            gameManager.GameOver();
            
        }
    }

    

 
}
