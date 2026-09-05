using UnityEngine;

/// <summary>
/// Pokretna traka u tvornici (razina 4). Dok Slavko stoji na njoj, njegova se
/// brzina trcanja mnozi zadanim faktorom: traka koja ide njemu ususret usporava ga
/// (faktor &lt; 1), a ona koja ide u njegovom smjeru ubrzava (faktor &gt; 1).
///
/// Usporavajuca traka nije samo smetnja: dok je Slavko na njoj, cuvar mu se
/// primice, pa pogreska na traci stoji vise nego pogreska na tlu.
///
/// Collider2D na ovom objektu mora biti "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Mnozitelj brzine trcanja dok je Slavko na traci. " +
             "Manje od 1 = traka ide njemu ususret, vise od 1 = nosi ga naprijed.")]
    public float speedFactor = 0.8f;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Apply(other, speedFactor);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Apply(other, 1f);
    }

    private void OnDisable()
    {
        // Ako se traka ugasi ili unisti dok je Slavko na njoj, OnTriggerExit2D se
        // nece javiti, pa bi mu faktor ostao zauvijek promijenjen.
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetEnvironmentSpeedFactor(1f);
        }
    }

    private static void Apply(Collider2D other, float factor)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.SetEnvironmentSpeedFactor(factor);
    }
}
