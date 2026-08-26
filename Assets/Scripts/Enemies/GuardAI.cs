using UnityEngine;

/// <summary>
/// Jednostavan "rubber-band" AI za čuvara: ako igrač preveć odmakne, čuvar ubrzava
/// da ga dostigne, a inače se kreće osnovnom brzinom. Kad dotakne Slavka (trigger),
/// nanosi mu udarac preko PlayerHealth.TakeHit().
/// Postavi Collider2D na ovom objektu kao "Is Trigger".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GuardAI : MonoBehaviour
{
    [Header("Referenca na igrača (ostavi prazno za automatsko pronalaženje po tagu 'Player')")]
    public Transform player;

    [Header("Brzina (postavlja se i iz LevelManager-a po razini)")]
    public float baseSpeed = 5.5f;
    public float catchUpSpeed = 8f;
    [Tooltip("Ako je igrač dalje od čuvara od ove udaljenosti, čuvar ubrzava")]
    public float catchUpDistance = 6f;

    [Header("Reakcija na uhvaćanje")]
    [Tooltip("Koliko dugo čuvar stane na mjestu nakon što dotakne Slavka, prije nego nastavi potjeru")]
    public float catchPauseDuration = 0.5f;

    private Rigidbody2D rb;
    private float pausedUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        if (Time.time < pausedUntil)
        {
            // Čuvar je upravo uhvatio Slavka — stoji na mjestu umjesto da prođe kroz njega.
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float distance = player.position.x - transform.position.x;
        float targetSpeed = distance > catchUpDistance ? catchUpSpeed : baseSpeed;

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeHit();
        }

        // Zastani nakratko na mjestu umjesto da nastaviš kliziti kroz Slavka —
        // on se u međuvremenu odbija unatrag (knockback iz PlayerHealth.TakeHit).
        pausedUntil = Time.time + catchPauseDuration;
    }
}
