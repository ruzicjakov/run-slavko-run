using UnityEngine;

/// <summary>
/// Prepreka koju Slavko može probiti dok je aktivan PowerApe power-up.
/// Bez aktivnog PowerApe-a, sudar s ovom preprekom oduzima život kao i obična prepreka.
/// </summary>
public class BreakableObstacle : MonoBehaviour
{
    [Tooltip("Opcionalni efekt (npr. čestice/krhotine) koji se instancira kad se prepreka razbije")]
    public GameObject breakEffect;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var player = collision.collider.GetComponent<PlayerController>();

        if (player != null && player.CanBreakObstacles)
        {
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
            return;
        }

        var health = collision.collider.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeHit();
        }
    }
}
