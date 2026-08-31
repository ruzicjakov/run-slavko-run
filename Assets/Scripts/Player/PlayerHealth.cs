using System.Collections;
using UnityEngine;

/// <summary>
/// Upravlja životima Slavka. "extraLives" se povećava prikupljanjem Kokosa.
/// Zivot oduzima samo cuvar (TakeHit); sudar s preprekom Slavka posrne (Stumble).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Životi")]
    [Tooltip("Broj dodatnih života (bez ovoga, prvi pogodak = Game Over)")]
    public int extraLives = 0;
    [Tooltip("Najviše dodatnih života. Ukupno = ovo + 1 (osnovni), dakle 2 => najviše 3 života.")]
    public int maxExtraLives = 2;

    [Header("Nakon pogotka")]
    [Tooltip("Koliko dugo Slavko treperi i ne može primiti novi udarac")]
    public float invulnerabilityDuration = 1.5f;
    [Tooltip("Koliko dugo je auto-trčanje isključeno nakon udarca — kratko, samo da odbacivanje ima efekta")]
    public float knockbackDuration = 0.35f;
    public float knockbackForce = 4f;

    [Header("Posrtanje (sudar s preprekom)")]
    [Tooltip("Sudar s preprekom NE oduzima zivot, nego Slavka posrne i izgubi tlo. " +
             "Koliko dugo je auto-trcanje iskljuceno nakon sudara.")]
    public float stumbleStopDuration = 0.22f;
    [Tooltip("Koliko jako sudar s preprekom odgurne Slavka unatrag")]
    public float stumbleNudge = 0.8f;
    [Tooltip("Najmanji razmak izmedu dva posrtanja, da ista prepreka ne okine vise puta")]
    public float stumbleCooldown = 0.5f;

    private bool isInvulnerable;
    private float stumbleCooldownUntil;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        UIManager.Instance?.UpdateLives(extraLives + 1, maxExtraLives + 1);
    }

    /// <summary>Pozvati iz Kokos power-upa.</summary>
    public void AddExtraLife()
    {
        extraLives = Mathf.Min(extraLives + 1, maxExtraLives);
        UIManager.Instance?.UpdateLives(extraLives + 1, maxExtraLives + 1);
    }

    /// <summary>
    /// Sudar s preprekom. NE oduzima zivot: Slavko posrne, izgubi malo tla i cuvar mu
    /// se primakne. Zivot oduzima tek cuvar, ako to izgubljeno tlo uspije iskoristiti.
    /// Vraca true ako je posrtanje stvarno primijenjeno (tada se prepreka mice s puta).
    /// </summary>
    public bool Stumble()
    {
        if (isInvulnerable) return false;
        if (Time.time < stumbleCooldownUntil) return false;

        stumbleCooldownUntil = Time.time + stumbleCooldown;
        AudioManager.Instance?.PlayHit();

        if (rb != null)
        {
            // Namjerno postavljamo brzinu umjesto impulsa: iznos gubitka tla mora biti
            // isti bez obzira na to kojom je brzinom Slavko udario u prepreku.
            rb.linearVelocity = new Vector2(-stumbleNudge, rb.linearVelocity.y);
            playerController?.NotifyKnockback(stumbleStopDuration);
        }

        return true;
    }

    /// <summary>
    /// Pozvati kad čuvar uhvati Slavka ili kad udari u prepreku koja nanosi štetu.
    /// Vraća true ako je udarac stvarno primljen, a false ako je Slavko bio nepovrediv
    /// (prepreke to koriste da ne nestanu bez razloga).
    /// </summary>
    /// <param name="pushDirX">
    /// Smjer odbacivanja: -1 unatrag (prepreka ispred), +1 naprijed (čuvar iza).
    /// Bez ovoga bi čuvar odbacivao Slavka ravno u sebe i odmah ga ponovno pogodio.
    /// </param>
    public bool TakeHit(float pushDirX = -1f)
    {
        if (isInvulnerable) return false;

        AudioManager.Instance?.PlayHit();

        if (extraLives > 0)
        {
            extraLives--;
            UIManager.Instance?.UpdateLives(extraLives + 1, maxExtraLives + 1);
            StartCoroutine(InvulnerabilityRoutine());

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                float dir = pushDirX >= 0f ? 1f : -1f;
                rb.AddForce(new Vector2(dir * knockbackForce, knockbackForce * 0.5f),
                            ForceMode2D.Impulse);
                // Samo kratko gasimo auto-trčanje — nepovredivost traje dulje, ali kontrolu
                // vraćamo brzo da se Slavko ne osjeća "mrtvo" 1.5 sekundi nakon svakog udarca.
                playerController?.NotifyKnockback(knockbackDuration);
            }
            return true;
        }

        GameManager.Instance?.GameOver();
        return true;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        // Jednostavno vizualno treperenje dok je Slavko nepovrediv.
        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvulnerable = false;
    }
}
