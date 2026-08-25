using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Upravlja HUD-om i ekranima (Game Over, Level Complete, Victory).
/// Stavi ovu skriptu na GameObject u sceni razine (npr. "UIManager" unutar Canvasa)
/// i povuci odgovarajuće Text/Panel referencе u Inspectoru.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Text livesText;
    public Text powerUpTimerText;

    [Header("Ekrani (Panel GameObjecti, isključeni po defaultu)")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject victoryPanel;

    private Action pendingNextLevelAction;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (powerUpTimerText != null) powerUpTimerText.text = "";
    }

    public void UpdateLives(int extraLives)
    {
        if (livesText != null)
        {
            livesText.text = "Životi: " + extraLives;
        }
    }

    public void ShowPowerUpTimer(string powerUpName, float duration)
    {
        if (powerUpTimerText == null) return;
        StopCoroutine(nameof(PowerUpTimerRoutine));
        StartCoroutine(PowerUpTimerRoutine(powerUpName, duration));
    }

    private IEnumerator PowerUpTimerRoutine(string powerUpName, float duration)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            powerUpTimerText.text = powerUpName + ": " + remaining.ToString("F1") + "s";
            yield return null;
            remaining -= Time.deltaTime;
        }
        powerUpTimerText.text = "";
    }

    // ---------- Game Over ----------

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    /// <summary>Zakači na OnClick gumba "Retry" na Game Over ekranu.</summary>
    public void OnRetryButton()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance?.RestartLevel();
    }

    /// <summary>Zakači na OnClick gumba "Main Menu".</summary>
    public void OnMainMenuButton()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    // ---------- Level Complete ----------

    public void ShowLevelComplete(Action onNextLevel)
    {
        pendingNextLevelAction = onNextLevel;
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    /// <summary>Zakači na OnClick gumba "Next Level".</summary>
    public void OnNextLevelButton()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        pendingNextLevelAction?.Invoke();
    }

    // ---------- Victory ----------

    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }
}
