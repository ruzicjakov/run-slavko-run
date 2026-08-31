using UnityEngine;

/// <summary>
/// Prepreka koju Slavko može probiti dok je aktivan PowerApe power-up.
/// Bez aktivnog PowerApe-a, sudar s ovom preprekom oduzima život kao i obična prepreka —
/// ali se i tada makne s puta, da Slavko ne ostane zaglavljen ispred nje.
/// </summary>
public class BreakableObstacle : MonoBehaviour
{
    [Tooltip("Opcionalni efekt (npr. čestice/krhotine) koji se instancira kad se prepreka razbije")]
    public GameObject breakEffect;

    [Tooltip("Koliko sekundi nakon udarca prepreka nestaje (collider se gasi odmah)")]
    public float removeDelay = 0.25f;

    private bool alreadyHit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (alreadyHit) return;
        if (!collision.collider.CompareTag("Player")) return;

        var player = collision.collider.GetComponent<PlayerController>();

        if (player != null && player.CanBreakObstacles)
        {
            alreadyHit = true;
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
            return;
        }

        var health = collision.collider.GetComponent<PlayerHealth>();
        if (health == null) return;

        if (!health.Stumble()) return;

        alreadyHit = true;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, removeDelay);
    }
}
