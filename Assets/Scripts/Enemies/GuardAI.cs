using UnityEngine;

/// <summary>
/// Cuvar koji progoni Slavka.
///
/// Model je "rubber-band": ako Slavko odmakne dalje od catchUpDistance, cuvar sprinta,
/// a inace trci osnovnom brzinom, koja je namjerno malo manja od Slavkove. Zbog toga se
/// razmak ustali oko catchUpDistance, sto je znatno vise od dohvata — u cistoj voznji
/// cuvar NIKAD ne uhvati Slavka, i to je namjerno.
///
/// Cuvar postaje opasan tek kad Slavko izgubi tlo. Svaki sudar s preprekom ga posrne
/// (PlayerHealth.Stumble) i razmak skokovito padne. Jedno posrtanje cuvar ne stigne
/// iskoristiti, ali drugo posrtanje prije nego se razmak oporavi znaci da cuvar ude u
/// dohvat i oduzme zivot.
///
/// Hvatanje se provjerava PO UDALJENOSTI, a ne preko trigger dogadaja. Trigger se okine
/// samo pri ulasku, pa je cuvar prije znao stajati Slavku za vratom a da ga nikad ne
/// pogodi: nepovredivost bi istekla tek kad se collideri vise ne dodiruju.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GuardAI : MonoBehaviour
{
    [Header("Referenca na igraca (ostavi prazno za automatsko pronalazenje po tagu 'Player')")]
    public Transform player;

    [Header("Brzina (postavlja se i iz LevelManager-a po razini)")]
    public float baseSpeed = 5.5f;
    public float catchUpSpeed = 8f;
    [Tooltip("Ako je igrac dalje od cuvara od ove udaljenosti, cuvar ubrzava")]
    public float catchUpDistance = 3f;

    [Header("Zaustavljanje kod igraca")]
    [Tooltip("Na kojoj udaljenosti iza Slavka se cuvar zaustavlja. Nikad ne ide dalje od toga.")]
    public float stopDistance = 0.9f;

    [Header("Hvatanje")]
    [Tooltip("Na kojoj udaljenosti cuvar zgrabi Slavka. Mora biti malo vece od stopDistance, " +
             "inace hvatanje ovisi o tome dodiruju li se collideri bas u tom kadru.")]
    public float reachDistance = 1.05f;

    [Tooltip("Najveca razlika u visini pri kojoj cuvar jos moze zgrabiti Slavka. " +
             "Sprjecava hvatanje dok Slavko visi na uzetu iznad cuvara.")]
    public float reachHeight = 1.5f;

    [Tooltip("Koliko dugo cuvar stoji nakon sto uspjesno pogodi Slavka. Bez toga bi ga " +
             "drzao i pogadao iznova svaki put kad istekne nepovredivost.")]
    public float recoilDuration = 0.7f;

    [Header("Nalet (uvedeno u razini 4)")]
    [Tooltip("Svakoliko sekundi cuvar krene u nalet. 0 iskljucuje nalete.")]
    public float burstInterval;

    [Tooltip("Koliko nalet traje")]
    public float burstDuration = 1f;

    [Tooltip("Brzina tijekom naleta. Tijekom naleta cuvar ne postuje granicu " +
             "catchUpDistance, nego juri punom brzinom.")]
    public float burstSpeed = 9f;

    private Rigidbody2D rb;
    private PlayerHealth playerHealth;
    private PlayerController playerController;
    private float recoilUntil;
    private float nextBurstAt = -1f;
    private float burstUntil;

    /// <summary>Je li cuvar trenutno u naletu — koristi se za vizualnu najavu.</summary>
    public bool IsBursting => Time.time < burstUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerController = player.GetComponent<PlayerController>();
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Nakon uspjesnog pogotka cuvar nakratko stoji i pusta Slavka da pobjegne.
        if (Time.time < recoilUntil)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Tvrda granica: cuvar se ne smije naci ispred ove tocke. Pouzdanije je od
        // oslanjanja na sudare, jer vrijedi u svakom koraku fizike.
        float limitX = player.position.x - stopDistance;

        if (transform.position.x >= limitX)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (transform.position.x > limitX)
            {
                transform.position = new Vector3(limitX, transform.position.y, transform.position.z);
            }

            TryHit();
            return;
        }

        float distance = player.position.x - transform.position.x;

        // Nalet: cuvar povremeno pojuri punom brzinom bez obzira na razmak. Sam po sebi
        // ne uspije uhvatiti Slavka, ali mu se toliko priblizi da svaka pogreska
        // napravljena tijekom naleta znaci gubitak zivota.
        float targetSpeed;
        // Nalet i usporavajuca traka ne smiju se preklapati. Oba pritiska smanjuju
        // razmak, a zajedno ga smanje toliko da cuvar uhvati Slavka bez ijedne
        // njegove pogreske. Zato traka ima prednost: dok je Slavko na njoj, cuvar
        // nalet niti pokrece niti nastavlja.
        bool slowed = playerController != null &&
                      playerController.EnvironmentSpeedFactor < 0.95f;

        if (slowed)
        {
            burstUntil = 0f;
        }

        if (burstInterval > 0f)
        {
            if (nextBurstAt < 0f) nextBurstAt = Time.time + burstInterval;

            if (Time.time >= nextBurstAt)
            {
                if (slowed)
                {
                    nextBurstAt = Time.time + 0.5f;
                }
                else
                {
                    burstUntil = Time.time + burstDuration;
                    nextBurstAt = Time.time + burstInterval;
                }
            }
        }

        if (Time.time < burstUntil)
        {
            targetSpeed = burstSpeed;
        }
        else
        {
            targetSpeed = distance > catchUpDistance ? catchUpSpeed : baseSpeed;
        }

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        if (distance <= reachDistance &&
            Mathf.Abs(player.position.y - transform.position.y) <= reachHeight)
        {
            TryHit();
        }
    }

    private void TryHit()
    {
        if (playerHealth == null) return;

        // +1 = odbaci Slavka NAPRIJED, dalje od cuvara. Da ga odbacujemo unatrag,
        // letio bi ravno u cuvara i bio bi pogoden iznova.
        if (playerHealth.TakeHit(1f))
        {
            recoilUntil = Time.time + recoilDuration;
        }
    }
}
