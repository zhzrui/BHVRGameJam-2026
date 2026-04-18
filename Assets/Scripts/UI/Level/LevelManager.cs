using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;
        Debug.Log("Scene loaded: " + scene.name + " at index: " + currentLevelIndex);

        //if (PartStateManager.Instance != null)
        //    PartStateManager.Instance.ClearAllParts();
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;
        
        Debug.Log("Current Level Index: " + currentLevelIndex);
        Debug.Log("Next Level Index: " + nextLevel);
        Debug.Log("Total Scenes in Build: " + SceneManager.sceneCountInBuildSettings);
        
        if (nextLevel < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Loading scene at index: " + nextLevel);
            SceneManager.LoadScene(nextLevel);
            // currentLevelIndex will be updated by OnSceneLoaded
        }
        else
        {
            Debug.Log("No more levels!");
        }
    }

    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public string GetCurrentLevelName()
    {
        return SceneManager.GetActiveScene().name;
    }
}