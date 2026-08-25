using UnityEngine;

/// <summary>
/// Jednostavan zvučni "singleton". Stavi na GameObject u prvoj sceni (npr. isti
/// GameObject kao GameManager), dodaj AudioSource komponente i povuci klipove.
/// Svi pozivi su null-safe (AudioManager.Instance?.PlayJump()) pa igra radi i bez zvuka.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source komponente (dodaj dvije na ovaj GameObject)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Zvučni efekti")]
    public AudioClip jumpClip;
    public AudioClip pickupClip;
    public AudioClip hitClip;
    public AudioClip gameOverClip;

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

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayJump() => PlaySfx(jumpClip);
    public void PlayPickup() => PlaySfx(pickupClip);
    public void PlayHit() => PlaySfx(hitClip);
    public void PlayGameOver() => PlaySfx(gameOverClip);

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
