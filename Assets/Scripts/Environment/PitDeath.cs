using UnityEngine;

/// <summary>
/// Postavi na širok trigger ISPOD cijele razine (ispod svih jama). Ako Slavko upadne
/// u jamu i ne uspije se prebaciti (swing preko HangPointa ili skok), razina odmah
/// završava — kao u pravim runner igrama gdje pad u ponor znači trenutnu smrt,
/// bez obzira na preostale dodatne živote.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PitDeath : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.GameOver();
        }
    }
}
