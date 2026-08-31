using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Globalni "singleton" koji upravlja tijekom igre: redoslijedom razina,
/// Game Over/Victory stanjem i prijelazima izmedu scena.
/// Stavi ovu skriptu na jedan GameObject u PRVOJ sceni (npr. MainMenu) — prezivljava
/// prelazak scena zahvaljujuci DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>Kljuc pod kojim se u PlayerPrefs pamti dokle je igrac stigao.</summary>
    private const string SaveKey = "SlavkoSavedLevel";

    [Header("Redoslijed razina (nazivi scena tocno kao u Build Settings)")]
    public string[] levelScenes =
    {
        "SampleScene",
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

    // ---------- Spremljeni napredak ----------

    /// <summary>Postoji li spremljena razina na koju se moze nastaviti.</summary>
    public static bool HasSavedProgress()
    {
        return PlayerPrefs.GetInt(SaveKey, 0) > 0;
    }

    /// <summary>Redni broj spremljene razine za prikaz (1-based), 0 ako nema.</summary>
    public static int SavedLevelNumber()
    {
        return PlayerPrefs.GetInt(SaveKey, 0) + 1;
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(SaveKey, currentLevelIndex);
        PlayerPrefs.Save();
    }

    private static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    // ---------- Pokretanje ----------

    /// <summary>Nova igra od prve razine (gumb "Igraj").</summary>
    public void StartGame()
    {
        ClearProgress();
        LoadLevelAt(0);
    }

    /// <summary>Nastavak od zadnje dosegnute razine (gumb "Nastavi").</summary>
    public void ContinueGame()
    {
        int saved = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 0), 0, levelScenes.Length - 1);
        LoadLevelAt(saved);
    }

    private void LoadLevelAt(int index)
    {
        currentLevelIndex = index;
        IsGameOver = false;
        Time.timeScale = 1f;
        Checkpoint.ResetCheckpoint();
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    /// <summary>Izlaz iz igre (gumb "Izadi"). U Editoru samo zaustavlja Play Mode.</summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Tijek igre ----------

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
            // Igra je predena — napredak vise nema smisla cuvati.
            ClearProgress();
            Time.timeScale = 0f;
            UIManager.Instance?.ShowVictory();
        }
        else
        {
            SaveProgress();
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
