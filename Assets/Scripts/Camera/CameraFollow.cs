using UnityEngine;

/// <summary>
/// Kamera koja glatko prati Slavka u "runner" stilu — pretežno horizontalno.
/// Stavi na Main Camera i povuci Playera kao target.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(2f, 1f, -10f);
    public float smoothSpeed = 5f;
    [Tooltip("Ako je true, kamera prati i vertikalno kretanje (npr. skokovi/swing)")]
    public bool followY = false;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        if (!followY)
        {
            desired.y = offset.y;
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
