using UnityEngine;

/// <summary>
/// Zvučni "singleton". Stavi na GameObject u prvoj sceni (isti objekt kao GameManager).
/// AudioSource komponente se stvaraju automatski u Awake, pa ih ne treba dodavati ručno.
/// Svi pozivi su null-safe (AudioManager.Instance?.PlayJump()) pa igra radi i bez zvuka.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source komponente (stvaraju se same ako su prazne)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Glasnoća")]
    [Range(0f, 1f)] public float musicVolume = 0.35f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

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

        // Komponente radimo kodom umjesto da ih postavljamo u sceni — tako ih ne
        // treba održavati u pet scena i nema šanse da negdje nedostaju.
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayJump() => PlaySfx(jumpClip);
    public void PlayPickup() => PlaySfx(pickupClip);
    public void PlayHit() => PlaySfx(hitClip);
    public void PlayGameOver() => PlaySfx(gameOverClip);

    /// <summary>
    /// Pušta glazbenu podlogu. Ako ista podloga već svira, ne pokreće je ispočetka —
    /// zato ponovni pokušaj razine ne prekida glazbu.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}
