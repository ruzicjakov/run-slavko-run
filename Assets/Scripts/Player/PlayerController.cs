using System.Collections;
using UnityEngine;

/// <summary>
/// Glavna skripta za kontrolu Slavka: automatsko trčanje, skok i vješanje/zamah (swing)
/// preko grana, užeta i bandera. Zakači ovu skriptu na Player GameObject koji ima
/// Rigidbody2D i Collider2D (npr. CapsuleCollider2D ili BoxCollider2D).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Trčanje")]
    [Tooltip("Osnovna brzina automatskog trčanja")]
    public float runSpeed = 6f;

    [Header("Skok")]
    public float jumpForce = 9f;
    [Tooltip("Prazan GameObject postavljen na 'noge' Slavka, koristi se za provjeru je li na tlu")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    [Tooltip("Layer koji predstavlja tlo/platforme")]
    public LayerMask groundLayer;

    [Header("Vješanje / Swing")]
    [Tooltip("Tipka koju igrač drži da bi se uhvatio za granu/uže/banderu")]
    public KeyCode grabKey = KeyCode.LeftShift;
    [Tooltip("Množitelj brzine pri otpuštanju zamaha (mali 'boost' pri iskakanju iz swinga)")]
    public float swingReleaseBoost = 1.15f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float speedMultiplier = 1f;
    private bool canBreakObstacles;

    private bool isHanging;
    private HingeJoint2D hingeJoint;
    private HangPoint activeHangPoint;
    private Transform activeHangTransform;
    private LineRenderer ropeRenderer;

    [Header("Vizualni konop (swing)")]
    [Tooltip("Boja i debljina konopa koji se crta dok Slavko visi/se njiše")]
    public float ropeWidth = 0.08f;
    public Color ropeColor = Color.black;

    private float runSuppressedUntil;

    private Coroutine speedBoostRoutine;
    private Coroutine powerApeRoutine;

    public bool IsHanging => isHanging;
    public bool CanBreakObstacles => canBreakObstacles;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isHanging)
        {
            HandleRun();
            HandleJumpInput();
            HandleGrabInput();
        }
        else
        {
            HandleSwingRelease();
            UpdateRopeVisual();
        }
    }

    private void HandleRun()
    {
        // Nakon udarca (knockback) auto-trčanje se nakratko isključuje (vidi NotifyKnockback)
        // da udarac stvarno odgurne Slavka umjesto da ga ova linija odmah povuče natrag u prepreku.
        if (Time.time < runSuppressedUntil) return;

        // NAPOMENA: u Unity 6 "Rigidbody2D.velocity" je obilježen kao obsolete u korist
        // "linearVelocity", ali i dalje radi. Ako koristiš Unity 6+ i želiš bez upozorenja,
        // zamijeni rb.velocity -> rb.linearVelocity na svim mjestima u ovoj datoteci.
        rb.linearVelocity = new Vector2(runSpeed * speedMultiplier, rb.linearVelocity.y);
    }

    private void HandleJumpInput()
    {
        bool jumpPressed = Input.GetButtonDown("Jump") ||
                            Input.GetKeyDown(KeyCode.W) ||
                            Input.GetKeyDown(KeyCode.UpArrow);

        if (isGrounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            AudioManager.Instance?.PlayJump();
        }
    }

    private void HandleGrabInput()
    {
        if (activeHangPoint != null && Input.GetKey(grabKey))
        {
            StartHanging(activeHangPoint);
        }
    }

    private void HandleSwingRelease()
    {
        bool releasePressed = Input.GetKeyUp(grabKey) || Input.GetButtonDown("Jump");
        if (releasePressed)
        {
            StopHanging();
        }
    }

    /// <summary>Poziva HangPoint kad Slavko uđe u njegov trigger.</summary>
    public void SetHangPointAvailable(HangPoint point)
    {
        activeHangPoint = point;
    }

    /// <summary>Poziva HangPoint kad Slavko izađe iz njegovog triggera.</summary>
    public void ClearHangPoint(HangPoint point)
    {
        if (activeHangPoint == point)
        {
            activeHangPoint = null;
        }
    }

    private void StartHanging(HangPoint point)
    {
        isHanging = true;
        activeHangPoint = null;
        activeHangTransform = point.transform;

        // Dinamički dodajemo HingeJoint2D vezan na fiksnu točku u prostoru (grana/uže/bandera).
        // Time Slavko počinje "swingati" oko te točke kao klatno.
        hingeJoint = gameObject.AddComponent<HingeJoint2D>();
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.connectedAnchor = point.transform.position;
        hingeJoint.anchor = Vector2.zero;
        hingeJoint.useLimits = false;

        // Vizualni "konop" između Slavka i točke vješanja dok traje swing.
        ropeRenderer = gameObject.AddComponent<LineRenderer>();
        ropeRenderer.positionCount = 2;
        ropeRenderer.startWidth = ropeWidth;
        ropeRenderer.endWidth = ropeWidth;
        ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        ropeRenderer.startColor = ropeColor;
        ropeRenderer.endColor = ropeColor;
        ropeRenderer.sortingOrder = 5;
        ropeRenderer.useWorldSpace = true;
        UpdateRopeVisual();
    }

    private void StopHanging()
    {
        isHanging = false;
        activeHangTransform = null;

        if (hingeJoint != null)
        {
            Destroy(hingeJoint);
            hingeJoint = null;
        }

        if (ropeRenderer != null)
        {
            Destroy(ropeRenderer);
            ropeRenderer = null;
        }

        // Mali boost u smjeru trenutnog zamaha kod otpuštanja (osjećaj "leta" nakon swinga).
        rb.linearVelocity *= swingReleaseBoost;
    }

    private void UpdateRopeVisual()
    {
        if (ropeRenderer == null || activeHangTransform == null) return;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, activeHangTransform.position);
    }

    /// <summary>
    /// Pozvati iz PlayerHealth.TakeHit() kad se primjenjuje knockback — nakratko isključuje
    /// automatsko trčanje da odgurivanje unatrag stvarno odvoji Slavka od prepreke, umjesto
    /// da HandleRun() svaki frame odmah vrati brzinu prema naprijed i zaglavi ga na mjestu.
    /// </summary>
    public void NotifyKnockback(float duration)
    {
        runSuppressedUntil = Time.time + duration;
    }

    // ---------- Power-up hookovi (pozivaju se iz PowerUps skripti) ----------

    /// <summary>Banana Boost: privremeno povećava brzinu trčanja.</summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostRoutine != null) StopCoroutine(speedBoostRoutine);
        speedBoostRoutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        UIManager.Instance?.ShowPowerUpTimer("Banana Boost", duration);
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
    }

    /// <summary>PowerApe: privremeno omogućuje probijanje lomljivih prepreka.</summary>
    public void ApplyPowerApe(float duration)
    {
        if (powerApeRoutine != null) StopCoroutine(powerApeRoutine);
        powerApeRoutine = StartCoroutine(PowerApeRoutine(duration));
    }

    private IEnumerator PowerApeRoutine(float duration)
    {
        canBreakObstacles = true;
        UIManager.Instance?.ShowPowerUpTimer("PowerApe", duration);
        yield return new WaitForSeconds(duration);
        canBreakObstacles = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
