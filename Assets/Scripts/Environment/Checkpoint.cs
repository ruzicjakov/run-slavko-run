using UnityEngine;

/// <summary>
/// Postavi na trigger objekte duž razine kao "checkpointe". Ako Slavko izgubi sve
/// dodatne živote i mora ponoviti razinu, respawna se na posljednjem prođenom checkpointu
/// umjesto na samom početku (vidi LevelManager.Start()).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private static Vector3? lastCheckpointPosition;
    public static Vector3? LastCheckpointPosition => lastCheckpointPosition;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lastCheckpointPosition = transform.position;
        }
    }

    /// <summary>Pozvati pri prelasku na novu razinu ili povratku u glavni izbornik.</summary>
    public static void ResetCheckpoint()
    {
        lastCheckpointPosition = null;
    }
}
