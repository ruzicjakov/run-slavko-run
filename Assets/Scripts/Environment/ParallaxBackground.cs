using UnityEngine;

/// <summary>
/// Paralaksno scrollanje pozadinskih slojeva — daj svakom sloju drugačiji parallaxFactor
/// da dobiješ dojam dubine (npr. daleko nebo = 0.1, srednji plan = 0.4, prednji plan = 0.8).
/// Stavi na svaki pozadinski sloj (SpriteRenderer) posebno.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0 = ne miče se (najdalji plan), 1 = miče se istom brzinom kao kamera")]
    public float parallaxFactor = 0.5f;

    private Transform cam;
    private Vector3 lastCamPosition;

    private void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
            lastCamPosition = cam.position;
        }
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 delta = cam.position - lastCamPosition;
        transform.position += new Vector3(delta.x * parallaxFactor, 0f, 0f);
        lastCamPosition = cam.position;
    }
}
