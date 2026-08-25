using UnityEngine;

/// <summary>
/// Postavke specifične za jednu razinu (scenu). Stavi na prazan GameObject u sceni
/// razine i povuci referencu na čuvara te podesi težinu.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Čuvar u ovoj razini (opcionalno, može ih biti i više)")]
    public GuardAI[] guards;

    [Header("Postavke težine ove razine")]
    public float guardBaseSpeed = 5.5f;
    public float guardCatchUpSpeed = 8f;

    private void Start()
    {
        foreach (var guard in guards)
        {
            if (guard == null) continue;
            guard.baseSpeed = guardBaseSpeed;
            guard.catchUpSpeed = guardCatchUpSpeed;
        }

        // Ako postoji spremljeni checkpoint iz prethodnog pokušaja (nakon Game Over -> Retry),
        // respawnaj igrača tamo umjesto na početku razine.
        if (Checkpoint.LastCheckpointPosition.HasValue)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = Checkpoint.LastCheckpointPosition.Value;
            }
        }
    }
}
