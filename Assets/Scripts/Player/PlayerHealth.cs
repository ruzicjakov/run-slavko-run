using System.Collections;
using UnityEngine;

/// <summary>
/// Upravlja životima Slavka. "extraLives" se povećava prikupljanjem Kokosa.
/// Kad čuvar ili prepreka pogode Slavka, poziva se TakeHit().
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Životi")]
    [Tooltip("Broj dodatnih života (bez ovoga, prvi pogodak = Game Over)")]
    public int extraLives = 0;
    public int maxExtraLives = 3;

    [Header("Nakon pogotka")]
    [Tooltip("Koliko dugo Slavko treperi i ne može primiti novi udarac")]
    public float invulnerabilityDuration = 1.5f;
    [Tooltip("Koliko dugo je auto-trčanje isključeno nakon udarca — kratko, samo da odbacivanje ima efekta")]
    public float knockbackDuration = 0.35f;
    public float knockbackForce = 4f;

    private bool isInvulnerable;
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
        UIManager.Instance?.UpdateLives(extraLives);
    }

    /// <summary>Pozvati iz Kokos power-upa.</summary>
    public void AddExtraLife()
    {
        extraLives = Mathf.Min(extraLives + 1, maxExtraLives);
        UIManager.Instance?.UpdateLives(extraLives);
    }

    /// <summary>
    /// Pozvati kad čuvar uhvati Slavka ili kad udari u prepreku koja nanosi štetu.
    /// Vraća true ako je udarac stvarno primljen, a false ako je Slavko bio nepovrediv
    /// (prepreke to koriste da ne nestanu bez razloga).
    /// </summary>
    public bool TakeHit()
    {
        if (isInvulnerable) return false;

        AudioManager.Instance?.PlayHit();

        if (extraLives > 0)
        {
            extraLives--;
            UIManager.Instance?.UpdateLives(extraLives);
            StartCoroutine(InvulnerabilityRoutine());

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(-knockbackForce, knockbackForce * 0.5f), ForceMode2D.Impulse);
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
