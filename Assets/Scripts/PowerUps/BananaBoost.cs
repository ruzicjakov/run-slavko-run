using UnityEngine;

/// <summary>Banana Boost — privremeno povećava brzinu trčanja Slavka.</summary>
public class BananaBoost : PowerUpBase
{
    public float speedMultiplier = 1.6f;
    public float duration = 5f;

    protected override void ApplyEffect(GameObject player)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplySpeedBoost(speedMultiplier, duration);
        }
    }
}
