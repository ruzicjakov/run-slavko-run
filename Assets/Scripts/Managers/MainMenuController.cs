using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sve sto glavni izbornik treba. Gumbi gadaju OVU skriptu, a ne GameManager izravno.
///
/// ZASTO: GameManager je DontDestroyOnLoad singleton. Kad se izbornik ucita drugi put
/// (povratkom iz razine), objekt "Systems" iz nove scene se u Awake-u unisti jer instanca
/// vec postoji. Gumbi koji su gadali tu komponentu ostanu pokazivati na unisten objekt
/// i prestanu raditi. Ova skripta zivi na Canvasu koji se nikad ne unistava, pa je
/// uvijek valjana meta, a poziv dalje ide preko GameManager.Instance.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Gumb Nastavi — bit ce onemogucen ako nema spremljene razine")]
    public Button continueButton;

    [Header("Ploce izbornika")]
    [Tooltip("Grupa s naslovom i gumbima — sakriva se dok je otvoren uvod ili zasluge")]
    public GameObject menuRoot;

    [Tooltip("Uvodna ploca: tko je Slavko, zasto bjezi i koji je cilj")]
    public GameObject introPanel;

    [Tooltip("Ploca sa zaslugama")]
    public GameObject creditsPanel;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = GameManager.HasSavedProgress();
        }

        if (introPanel != null) introPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (menuRoot != null) menuRoot.SetActive(true);
    }

    /// <summary>
    /// Gumb "Igraj". Nova igra ne pocinje odmah, nego se prvo prikaze uvodna ploca
    /// koja igracu daje kontekst price — bez nje igrac ne zna tko je Slavko ni zasto bjezi.
    /// Ako uvodna ploca nije postavljena, igra pocinje izravno.
    /// </summary>
    public void Play()
    {
        if (introPanel == null)
        {
            StartGameNow();
            return;
        }

        if (menuRoot != null) menuRoot.SetActive(false);
        introPanel.SetActive(true);
    }

    /// <summary>Gumb "Krenimo" na uvodnoj ploci.</summary>
    public void StartGameNow()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    /// <summary>Gumb "Zasluge" u izborniku.</summary>
    public void ShowCredits()
    {
        if (creditsPanel == null) return;
        if (menuRoot != null) menuRoot.SetActive(false);
        creditsPanel.SetActive(true);
    }

    /// <summary>Gumb "Natrag" na ploci uvoda ili zasluga.</summary>
    public void BackToMenu()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (menuRoot != null) menuRoot.SetActive(true);
    }

    /// <summary>Gumb "Nastavi" — od zadnje dosegnute razine.</summary>
    public void Continue()
    {
        if (GameManager.Instance != null) GameManager.Instance.ContinueGame();
    }

    /// <summary>Gumb "Izadi".</summary>
    public void Quit()
    {
        if (GameManager.Instance != null) GameManager.Instance.QuitGame();
    }
}
