using UnityEngine;

/// <summary>
/// Obična (nelomljiva) prepreka koju igrač mora preskočiti ili izbjeći.
/// Sudar s njom oduzima život, isto kao hvatanje od čuvara.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Tooltip("Ako je true, sudar sa Slavkom oduzima život")]
    public bool damagesPlayer = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!damagesPlayer) return;
        if (!collision.collider.CompareTag("Player")) return;

        var health = collision.collider.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeHit();
        }
    }
}
