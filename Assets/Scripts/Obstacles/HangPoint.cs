using UnityEngine;

/// <summary>
/// Točka za vješanje/zamah — postavi na grane, uže ili bandere.
/// Collider2D na ovom objektu MORA biti postavljen na "Is Trigger" (automatski se
/// postavlja u Reset() kad prvi put dodaš skriptu u Editoru).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HangPoint : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetHangPointAvailable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ClearHangPoint(this);
        }
    }
}
