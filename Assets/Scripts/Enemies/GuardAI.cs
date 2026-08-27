using UnityEngine;

/// <summary>
/// Jednostavan "rubber-band" AI za čuvara: ako se igrač previše odmakne, čuvar ubrzava
/// da ga dostigne, a inače se kreće osnovnom brzinom.
/// Čuvar NIKAD ne prolazi kroz Slavka — dođe do njega, stane iza njega i čeka.
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

    [Header("Zaustavljanje kod igrača")]
    [Tooltip("Na kojoj udaljenosti iza Slavka se čuvar zaustavlja. Nikad ne ide dalje od toga.")]
    public float stopDistance = 0.9f;

    private Rigidbody2D rb;

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

        // Tvrda granica: čuvar se ne smije naći ispred ove točke. Ovo je pouzdanije od
        // oslanjanja na trigger događaje — oni se okinu samo jednom pri ulasku, pa je čuvar
        // prije nastavljao kliziti kroz Slavka dok je ovaj bio zaglavljen ispred prepreke.
        float limitX = player.position.x - stopDistance;

        if (transform.position.x >= limitX)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (transform.position.x > limitX)
            {
                transform.position = new Vector3(limitX, transform.position.y, transform.position.z);
            }
            return;
        }

        float distance = player.position.x - transform.position.x;
        float targetSpeed = distance > catchUpDistance ? catchUpSpeed : baseSpeed;

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Dok čuvar drži Slavka, pokušava ga udariti i dalje. Bez ovoga bi čuvar koji stoji
        // na Slavku bio potpuno bezopasan, jer OnTriggerEnter2D okine samo jednom.
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeHit();
        }
    }
}
