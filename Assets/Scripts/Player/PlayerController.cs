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
    [Tooltip("Najkraća moguća duljina konopa")]
    public float minRopeLength = 1.2f;
    [Tooltip("Najdulja moguća duljina konopa")]
    public float maxRopeLength = 3f;
    [Tooltip("Stalna sila prema naprijed dok Slavko visi — održava zamah da ne stane u mjestu")]
    public float swingForce = 14f;
    [Tooltip("Nakon ovoliko sekundi visenja Slavko se automatski pušta (da se igra ne zaglavi)")]
    public float maxHangDuration = 2f;
    [Tooltip("Koliko dugo nakon puštanja se ista točka ne može ponovno uhvatiti")]
    public float regrabCooldown = 0.35f;
    [Tooltip("Koliko dugo nakon izlaska iz zone hvatanja se još uvijek može uhvatiti — oprost ako Shift stisneš malo prekasno")]
    public float grabGraceTime = 0.15f;
    [Tooltip("Množitelj vodoravne brzine pri otpuštanju zamaha")]
    public float swingReleaseBoost = 1.15f;
    [Tooltip("Okomiti izbačaj pri otpuštanju — pretvara zamah u skok preko jame")]
    public float releaseUpwardBoost = 7f;

    [Header("Vizualni konop (swing)")]
    [Tooltip("Boja i debljina konopa koji se crta dok Slavko visi/se njiše")]
    public float ropeWidth = 0.08f;
    public Color ropeColor = Color.black;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float speedMultiplier = 1f;
    private bool canBreakObstacles;

    private bool isHanging;
    private DistanceJoint2D ropeJoint;
    private HangPoint activeHangPoint;
    private HangPoint currentHangPoint;
    private HangPoint lastReleasedPoint;
    private HangPoint recentHangPoint;
    private float recentHangPointUntil;
    private Transform activeHangTransform;
    private float hangStartTime;
    private float regrabAvailableAt;

    private GameObject ropeObject;
    private LineRenderer ropeRenderer;
    private Material ropeMaterial;

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

    private void FixedUpdate()
    {
        // Blagi stalni potisak naprijed dok visi — bez ovoga se klatno ugasi i Slavko
        // samo visi u mjestu umjesto da ga zamah prenese preko jame.
        if (isHanging && rb != null)
        {
            rb.AddForce(Vector2.right * swingForce, ForceMode2D.Force);
        }
    }

    private void HandleRun()
    {
        // Nakon udarca (knockback) auto-trčanje se nakratko isključuje (vidi NotifyKnockback)
        // da udarac stvarno odgurne Slavka umjesto da ga ova linija odmah povuče natrag u prepreku.
        if (Time.time < runSuppressedUntil) return;

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
        if (!Input.GetKey(grabKey)) return;

        // Ako je upravo izašao iz zone, još kratko dopuštamo hvatanje (grabGraceTime) —
        // bez toga treba pogoditi točku u pikselu, s tim je hvatanje osjetno praštajuće.
        HangPoint target = activeHangPoint;
        if (target == null && recentHangPoint != null && Time.time < recentHangPointUntil)
        {
            target = recentHangPoint;
        }
        if (target == null) return;

        // Kratki cooldown da se ista točka ne uhvati odmah ponovno nakon puštanja.
        if (target == lastReleasedPoint && Time.time < regrabAvailableAt) return;

        StartHanging(target);
    }

    private void HandleSwingRelease()
    {
        bool releasePressed = Input.GetKeyUp(grabKey) || Input.GetButtonDown("Jump");
        bool timedOut = Time.time - hangStartTime >= maxHangDuration;

        if (releasePressed || timedOut)
        {
            StopHanging(true);
        }
    }

    /// <summary>Poziva HangPoint kad Slavko uđe u njegov trigger.</summary>
    public void SetHangPointAvailable(HangPoint point)
    {
        activeHangPoint = point;
        recentHangPoint = point;
        recentHangPointUntil = Time.time + grabGraceTime;
    }

    /// <summary>Poziva HangPoint kad Slavko izađe iz njegovog triggera.</summary>
    public void ClearHangPoint(HangPoint point)
    {
        if (activeHangPoint == point)
        {
            activeHangPoint = null;
            // Zapamti je nakratko — hvatanje ostaje moguće još grabGraceTime sekundi.
            recentHangPoint = point;
            recentHangPointUntil = Time.time + grabGraceTime;
        }
    }

    private void StartHanging(HangPoint point)
    {
        if (isHanging || point == null) return;

        isHanging = true;
        currentHangPoint = point;
        activeHangTransform = point.transform;
        hangStartTime = Time.time;

        Vector2 anchor = point.transform.position;
        float currentDistance = Vector2.Distance(transform.position, anchor);

        // DistanceJoint2D = konop fiksne duljine (klatno). VAŽNO: HingeJoint2D ovdje NE valja —
        // on spaja točku na tijelu izravno s točkom vješanja, pa se Slavko "zalijepi" na granu
        // umjesto da visi ispod nje na konopu.
        ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
        ropeJoint.autoConfigureDistance = false;
        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedBody = null;
        ropeJoint.connectedAnchor = anchor;
        ropeJoint.anchor = Vector2.zero;
        ropeJoint.distance = Mathf.Clamp(currentDistance, minRopeLength, maxRopeLength);
        ropeJoint.maxDistanceOnly = false;
        ropeJoint.enableCollision = false;

        CreateRopeVisual();
        UpdateRopeVisual();
    }

    private void StopHanging(bool applyLaunch)
    {
        if (!isHanging) return;

        isHanging = false;
        lastReleasedPoint = currentHangPoint;
        regrabAvailableAt = Time.time + regrabCooldown;
        currentHangPoint = null;
        activeHangTransform = null;

        if (ropeJoint != null)
        {
            Destroy(ropeJoint);
            ropeJoint = null;
        }

        DestroyRopeVisual();

        if (applyLaunch && rb != null)
        {
            // Zamah se pretvara u lansiranje naprijed-gore — to je ono što nosi Slavka preko jame.
            Vector2 v = rb.linearVelocity;
            float vx = Mathf.Max(Mathf.Abs(v.x), runSpeed * speedMultiplier) * swingReleaseBoost;
            float vy = Mathf.Max(v.y, releaseUpwardBoost);
            rb.linearVelocity = new Vector2(vx, vy);
        }
    }

    private void CreateRopeVisual()
    {
        // VAŽNO: LineRenderer NE smije ići na Player objekt — on već ima SpriteRenderer,
        // a Unity dopušta samo jedan Renderer po GameObjectu (zato se konop prije nije vidio).
        // Zato konop crtamo na zasebnom child objektu.
        if (ropeMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader != null) ropeMaterial = new Material(shader);
        }

        ropeObject = new GameObject("SwingRope");
        ropeObject.transform.SetParent(transform, false);

        ropeRenderer = ropeObject.AddComponent<LineRenderer>();
        ropeRenderer.positionCount = 2;
        ropeRenderer.useWorldSpace = true;
        ropeRenderer.startWidth = ropeWidth;
        ropeRenderer.endWidth = ropeWidth;
        ropeRenderer.numCapVertices = 4;
        ropeRenderer.startColor = ropeColor;
        ropeRenderer.endColor = ropeColor;
        ropeRenderer.textureMode = LineTextureMode.Stretch;
        ropeRenderer.alignment = LineAlignment.View;
        ropeRenderer.sortingOrder = 10;
        if (ropeMaterial != null) ropeRenderer.material = ropeMaterial;
    }

    private void DestroyRopeVisual()
    {
        if (ropeObject != null)
        {
            Destroy(ropeObject);
            ropeObject = null;
        }
        ropeRenderer = null;
    }

    private void UpdateRopeVisual()
    {
        if (ropeRenderer == null || activeHangTransform == null) return;
        ropeRenderer.SetPosition(0, activeHangTransform.position);
        ropeRenderer.SetPosition(1, transform.position);
    }

    /// <summary>
    /// Pozvati iz PlayerHealth.TakeHit() kad se primjenjuje knockback — nakratko isključuje
    /// automatsko trčanje da odgurivanje unatrag stvarno odvoji Slavka od prepreke, umjesto
    /// da HandleRun() svaki frame odmah vrati brzinu prema naprijed i zaglavi ga na mjestu.
    /// Ako je Slavko u tom trenutku visio, konop se prekida (bez izbačaja) da udarac ima učinka.
    /// </summary>
    public void NotifyKnockback(float duration)
    {
        if (isHanging) StopHanging(false);
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
