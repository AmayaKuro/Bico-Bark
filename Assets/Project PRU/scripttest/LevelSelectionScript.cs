using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionScript : MonoBehaviour
{
    public Button[] lvlButtons;

    public void OpenLevel(int levelIndex)
    {
        string levelName = "Level" + levelIndex.ToString();
        SceneManager.LoadScene(levelName);
    }
}
