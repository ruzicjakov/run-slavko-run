using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Upravlja HUD-om i ekranima (Game Over, Level Complete, Victory).
/// Zivoti se prikazuju kao srca gore lijevo, a trajanje power-upa kao traka
/// koja se prazni gore desno.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD — zivoti (srca, gore lijevo)")]
    [Tooltip("Slike srca, redom. Koliko ih ima, toliko je najvise zivota.")]
    public Image[] lifeIcons;
    public Color fullHeartColor = new Color(0.90f, 0.24f, 0.27f, 1f);
    public Color emptyHeartColor = new Color(0.27f, 0.29f, 0.33f, 1f);

    [Header("HUD — traka power-upa (gore desno)")]
    [Tooltip("Cijeli objekt trake — gasi se kad nema aktivnog power-upa")]
    public GameObject powerUpBarRoot;
    [Tooltip("Image s Image Type = Filled, Fill Method = Horizontal")]
    public Image powerUpFill;
    public Color powerApeColor = new Color(1f, 0.55f, 0.15f, 1f);
    public Color bananaColor = new Color(1f, 0.85f, 0.20f, 1f);

    [Header("Ekrani (Panel GameObjecti, iskljuceni po defaultu)")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject victoryPanel;

    private Action pendingNextLevelAction;
    private Coroutine powerUpRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        // Bez ovoga Instance nakon promjene scene pokazuje na unistenu komponentu,
        // pa "Instance?.Nesto()" baca MissingReferenceException umjesto da preskoci.
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (powerUpBarRoot != null) powerUpBarRoot.SetActive(false);
    }

    /// <summary>Pali srca do broja "current"; ostala zatamnjuje.</summary>
    public void UpdateLives(int current, int max)
    {
        if (lifeIcons == null) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] == null) continue;

            // Srca iznad dopustenog maksimuma se sakriju u potpunosti.
            bool exists = i < max;
            lifeIcons[i].enabled = exists;
            if (exists)
            {
                lifeIcons[i].color = i < current ? fullHeartColor : emptyHeartColor;
            }
        }
    }

    public void ShowPowerUpTimer(string powerUpName, float duration)
    {
        if (powerUpFill == null || powerUpBarRoot == null) return;

        // Cuvamo referencu na korutinu. StopCoroutine(nameof(...)) NE zaustavlja
        // korutinu pokrenutu izravnim pozivom, pa bi se kod dva power-upa zaredom
        // dvije korutine borile oko iste trake.
        if (powerUpRoutine != null) StopCoroutine(powerUpRoutine);
        powerUpRoutine = StartCoroutine(PowerUpTimerRoutine(powerUpName, duration));
    }

    private IEnumerator PowerUpTimerRoutine(string powerUpName, float duration)
    {
        powerUpBarRoot.SetActive(true);
        powerUpFill.color = powerUpName.StartsWith("Banana") ? bananaColor : powerApeColor;

        float remaining = duration;
        while (remaining > 0f)
        {
            powerUpFill.fillAmount = Mathf.Clamp01(remaining / duration);
            yield return null;
            remaining -= Time.deltaTime;
        }

        powerUpFill.fillAmount = 0f;
        powerUpBarRoot.SetActive(false);
        powerUpRoutine = null;
    }

    /// <summary>Je li otvoren neki od ekrana koji zaustavljaju igru.</summary>
    public bool AnyScreenOpen()
    {
        return (gameOverPanel != null && gameOverPanel.activeSelf)
            || (levelCompletePanel != null && levelCompletePanel.activeSelf)
            || (victoryPanel != null && victoryPanel.activeSelf);
    }

    // ---------- Game Over ----------

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    /// <summary>Zakaci na OnClick gumba "Retry" na Game Over ekranu.</summary>
    public void OnRetryButton()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance?.RestartLevel();
    }

    /// <summary>Zakaci na OnClick gumba "Main Menu".</summary>
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

    /// <summary>Zakaci na OnClick gumba "Next Level".</summary>
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
