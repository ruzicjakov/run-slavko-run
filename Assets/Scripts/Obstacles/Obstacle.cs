using UnityEngine;

/// <summary>
/// Obična (nelomljiva) prepreka koju igrač mora preskočiti ili izbjeći.
/// Sudar s njom oduzima život, isto kao hvatanje od čuvara.
/// Nakon što oduzme život, prepreka se miče s puta — inače bi Slavka auto-trčanje
/// vječno guralo natrag u nju i ostao bi zaglavljen ispred prepreke.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Tooltip("Ako je true, sudar sa Slavkom oduzima život")]
    public bool damagesPlayer = true;

    [Tooltip("Nakon oduzimanja života prepreka nestaje da Slavko može nastaviti dalje")]
    public bool disappearAfterHit = true;

    [Tooltip("Koliko sekundi nakon udarca prepreka nestaje (collider se gasi odmah)")]
    public float removeDelay = 0.25f;

    private bool alreadyHit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!damagesPlayer || alreadyHit) return;
        if (!collision.collider.CompareTag("Player")) return;

        var health = collision.collider.GetComponent<PlayerHealth>();
        if (health == null) return;

        // TakeHit vraća false ako je Slavko trenutno nepovrediv — tada prepreka ostaje.
        if (!health.TakeHit()) return;

        alreadyHit = true;
        if (disappearAfterHit) RemoveSelf();
    }

    private void RemoveSelf()
    {
        // Collider gasimo ODMAH da Slavko više nije blokiran, a objekt uklanjamo
        // trenutak kasnije da se nestanak vidi.
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, removeDelay);
    }
}
