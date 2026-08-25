using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Globalni "singleton" koji upravlja tijekom igre: redoslijedom razina,
/// Game Over/Victory stanjem i prijelazima između scena.
/// Stavi ovu skriptu na jedan GameObject u PRVOJ sceni (npr. MainMenu) — preživljava
/// prelazak scena zahvaljujući DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Redoslijed razina (nazivi scena točno kao u Build Settings)")]
    public string[] levelScenes =
    {
        "Level1_Zoo",
        "Level2_City",
        "Level3_Forest",
        "Level4_Factory"
    };

    [Header("Naziv scene glavnog izbornika")]
    public string mainMenuScene = "MainMenu";

    private int currentLevelIndex = 0;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Pokreni prvu razinu (npr. iz gumba "Start Game" na glavnom izborniku).</summary>
    public void StartGame()
    {
        currentLevelIndex = 0;
        IsGameOver = false;
        Time.timeScale = 1f;
        Checkpoint.ResetCheckpoint();
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayGameOver();
        UIManager.Instance?.ShowGameOver();
    }

    public void RestartLevel()
    {
        IsGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    public void CompleteLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= levelScenes.Length)
        {
            Time.timeScale = 0f;
            UIManager.Instance?.ShowVictory();
        }
        else
        {
            Time.timeScale = 0f;
            UIManager.Instance?.ShowLevelComplete(LoadNextLevel);
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        Checkpoint.ResetCheckpoint();
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        currentLevelIndex = 0;
        Checkpoint.ResetCheckpoint();
        SceneManager.LoadScene(mainMenuScene);
    }
}
