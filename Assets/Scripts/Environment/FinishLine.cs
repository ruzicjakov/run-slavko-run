using UnityEngine;

/// <summary>
/// Postavi na trigger objekt na samom kraju razine. Kad Slavko uđe u njega,
/// razina se smatra završenom.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinishLine : MonoBehaviour
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
            GameManager.Instance?.CompleteLevel();
        }
    }
}
