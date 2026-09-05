using UnityEngine;

/// <summary>
/// Prepreka koja se njise lijevo-desno oko svog pocetnog polozaja.
/// Uvedena u razini 2 (Grad): za razliku od nepomicnog sanduka, ovdje trenutak
/// skoka nije uvijek isti, jer se prepreka u meduvremenu pomakne.
///
/// Objekt MORA imati Rigidbody2D postavljen na Kinematic. Pomicanje statickog
/// collidera (bez Rigidbody2D) Unity ne ocekuje, pa zna propustiti sudar;
/// kinematicko tijelo pomaknuto preko MovePosition sudara se pouzdano.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingObstacle : MonoBehaviour
{
    [Tooltip("Koliko jedinica se prepreka pomakne od sredista u svaku stranu")]
    public float amplitude = 2f;

    [Tooltip("Trajanje jednog punog ciklusa naprijed-natrag, u sekundama")]
    public float period = 2.5f;

    [Tooltip("Pomak u ciklusu pri pokretanju, 0-1. Razlicite vrijednosti sprjecavaju " +
             "da se sve prepreke u razini njisu u istom ritmu.")]
    public float phase;

    private Rigidbody2D rb;
    private Vector2 origin;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        origin = transform.position;
    }

    private void FixedUpdate()
    {
        if (period <= 0f) return;

        float t = (Time.time / period + phase) * Mathf.PI * 2f;
        float offset = Mathf.Sin(t) * amplitude;
        rb.MovePosition(new Vector2(origin.x + offset, origin.y));
    }
}
