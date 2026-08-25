using UnityEngine;

/// <summary>PowerApe — privremeno omogućuje Slavku probijanje lomljivih prepreka.</summary>
public class PowerApe : PowerUpBase
{
    public float duration = 5f;

    protected override void ApplyEffect(GameObject player)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplyPowerApe(duration);
        }
    }
}
