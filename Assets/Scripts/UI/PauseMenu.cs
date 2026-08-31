using UnityEngine;

/// <summary>
/// Pauza na tipku Escape: zaustavlja igru i nudi nastavak, ponovni pokusaj razine
/// ili povratak u glavni izbornik.
///
/// Stavi na isti objekt gdje je UIManager (Canvas) i povuci PausePanel u polje.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("Panel koji se prikazuje na pauzi — iskljucen po defaultu")]
    public GameObject pausePanel;

    [Tooltip("Tipka za pauzu")]
    public KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;

        // Ne pauziramo ako je vec otvoren neki drugi ekran (Game Over, kraj razine,
        // pobjeda) — oni takoder drze Time.timeScale na 0 i nastavak bi ih pokvario.
        if (!IsPaused && UIManager.Instance != null && UIManager.Instance.AnyScreenOpen()) return;

        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    /// <summary>Zakaci na gumb "Nastavi".</summary>
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    /// <summary>Zakaci na gumb "Ponovi razinu".</summary>
    public void RestartLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        GameManager.Instance?.RestartLevel();
    }

    /// <summary>Zakaci na gumb "Glavni izbornik".</summary>
    public void ToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        GameManager.Instance?.ReturnToMainMenu();
    }
}
