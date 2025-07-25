using UnityEngine;

public class DisappearingManager : MonoBehaviour
{
    private static DisappearingManager instance;
    public static DisappearingManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DisappearingManager");
                instance = go.AddComponent<DisappearingManager>();
            }
            return instance;
        }
    }

    // Simple pause state
    public static bool IsPaused { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void PauseAll()
    {
        IsPaused = true;
        Debug.Log("All disappearing paused!");
    }

    public static void ResumeAll()
    {
        IsPaused = false;
        Debug.Log("All disappearing resumed!");
    }
}