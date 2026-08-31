using UnityEngine;

/// <summary>
/// Postavi na bilo koji objekt u sceni (npr. Canvas) i povuci glazbu te scene.
/// Pri pokretanju scene javlja AudioManageru koju podlogu treba svirati.
///
/// Razlog zašto ovo nije dio AudioManagera: AudioManager preživljava promjenu scene
/// (DontDestroyOnLoad), pa ne zna u kojoj je sceni. Ova skripta je vezana uz scenu
/// i zato zna svoju glazbu.
/// </summary>
public class LevelMusic : MonoBehaviour
{
    [Tooltip("Glazbena podloga za ovu scenu")]
    public AudioClip musicClip;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicClip, true);
        }
    }
}
