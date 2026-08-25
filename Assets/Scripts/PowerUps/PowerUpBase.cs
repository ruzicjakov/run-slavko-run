using UnityEngine;

/// <summary>
/// Zajednička osnova za sve power-upove: detektira dodir s igračem (trigger),
/// primjenjuje specifičan efekt te uklanja objekt sa scene.
/// Collider2D MORA biti "Is Trigger" (automatski postavljen u Reset()).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class PowerUpBase : MonoBehaviour
{
    [Tooltip("Opcionalni vizualni/zvučni efekt pri pokupljenju")]
    public GameObject pickupEffect;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);
        AudioManager.Instance?.PlayPickup();

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    /// <summary>Implementiraj u podklasi specifičan efekt power-upa.</summary>
    protected abstract void ApplyEffect(GameObject player);
}
