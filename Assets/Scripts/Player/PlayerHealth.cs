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
    public float invulnerabilityDuration = 1.5f;
    public float knockbackForce = 4f;

    private bool isInvulnerable;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

    /// <summary>Pozvati kad čuvar uhvati Slavka ili kad udari u prepreku koja nanosi štetu.</summary>
    public void TakeHit()
    {
        if (isInvulnerable) return;

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
            }
        }
        else
        {
            GameManager.Instance?.GameOver();
        }
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
