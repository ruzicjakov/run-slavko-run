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

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = GameManager.HasSavedProgress();
        }
    }

    /// <summary>Gumb "Igraj" — nova igra od prve razine.</summary>
    public void Play()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
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
