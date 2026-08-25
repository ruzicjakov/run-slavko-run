using UnityEngine;

/// <summary>Kokos — dodaje jedan dodatni život Slavku.</summary>
public class Kokos : PowerUpBase
{
    protected override void ApplyEffect(GameObject player)
    {
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.AddExtraLife();
        }
    }
}
