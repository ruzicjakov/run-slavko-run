using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pobjednicka animacija: naslov doskoci, panel se pojavi, a odozgo pada konfet.
///
/// VAZNO: kad se pobjeda prikaze, GameManager postavlja Time.timeScale = 0, pa je
/// obicni Time.deltaTime nula i nista se ne bi micalo. Zato sve ovdje ide na
/// Time.unscaledDeltaTime.
///
/// Stavi na VictoryPanel (objekt se pali preko UIManager.ShowVictory).
/// </summary>
public class VictoryAnimation : MonoBehaviour
{
    [Header("Naslov koji doskoci")]
    public RectTransform titleTransform;
    [Tooltip("Koliko traje doskok naslova")]
    public float popDuration = 0.55f;

    [Header("Konfet")]
    [Tooltip("Bilo koji mali sprite — koristi se obicna bijela plocica")]
    public Sprite confettiSprite;
    public int confettiCount = 70;
    [Tooltip("Koliko dugo konfet pada prije nego se ugasi (0 = beskonacno)")]
    public float confettiDuration = 0f;

    private readonly List<RectTransform> pieces = new List<RectTransform>();
    private readonly List<Vector2> velocities = new List<Vector2>();
    private readonly List<float> spins = new List<float>();
    private RectTransform selfRect;
    private float elapsed;

    private static readonly Color[] Palette =
    {
        new Color(0.95f, 0.77f, 0.20f, 1f),
        new Color(0.90f, 0.30f, 0.33f, 1f),
        new Color(0.30f, 0.72f, 0.95f, 1f),
        new Color(0.42f, 0.82f, 0.44f, 1f),
        new Color(0.85f, 0.50f, 0.90f, 1f),
    };

    private void OnEnable()
    {
        selfRect = GetComponent<RectTransform>();
        elapsed = 0f;
        SpawnConfetti();
        StopAllCoroutines();
        StartCoroutine(PopTitle());
    }

    private void OnDisable()
    {
        ClearConfetti();
    }

    private IEnumerator PopTitle()
    {
        if (titleTransform == null) yield break;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popDuration);
            // blagi "overshoot" pa smirivanje — naslov kao da doskoci
            float scale = 0.6f + 0.55f * Mathf.Sin(p * Mathf.PI * 0.5f) +
                          0.12f * Mathf.Sin(p * Mathf.PI * 2f) * (1f - p);
            titleTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        titleTransform.localScale = Vector3.one;
    }

    private void SpawnConfetti()
    {
        ClearConfetti();
        if (selfRect == null) return;

        float w = Mathf.Max(selfRect.rect.width, 800f);
        float h = Mathf.Max(selfRect.rect.height, 600f);

        for (int i = 0; i < confettiCount; i++)
        {
            var go = new GameObject("Confetti");
            go.transform.SetParent(transform, false);

            var img = go.AddComponent<Image>();
            img.sprite = confettiSprite;
            img.color = Palette[Random.Range(0, Palette.Length)];
            img.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(Random.Range(6f, 14f), Random.Range(10f, 20f));
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(Random.Range(-w / 2f, w / 2f),
                                               h / 2f + Random.Range(0f, h));
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            pieces.Add(rt);
            velocities.Add(new Vector2(Random.Range(-40f, 40f), Random.Range(-260f, -120f)));
            spins.Add(Random.Range(-180f, 180f));
        }
    }

    private void ClearConfetti()
    {
        foreach (var p in pieces)
        {
            if (p != null) Destroy(p.gameObject);
        }
        pieces.Clear();
        velocities.Clear();
        spins.Clear();
    }

    private void Update()
    {
        if (pieces.Count == 0) return;

        float dt = Time.unscaledDeltaTime;
        elapsed += dt;

        if (confettiDuration > 0f && elapsed > confettiDuration)
        {
            ClearConfetti();
            return;
        }

        float h = Mathf.Max(selfRect != null ? selfRect.rect.height : 600f, 600f);

        for (int i = 0; i < pieces.Count; i++)
        {
            var rt = pieces[i];
            if (rt == null) continue;

            Vector2 v = velocities[i];
            v.y -= 90f * dt;                                  // blaga gravitacija
            v.x += Mathf.Sin((elapsed + i) * 2f) * 12f * dt;  // lelujanje
            velocities[i] = v;

            rt.anchoredPosition += v * dt;
            rt.localRotation = rt.localRotation * Quaternion.Euler(0f, 0f, spins[i] * dt);

            // kad ispadne s ekrana, vrati ga gore — konfet pada neprekidno
            if (rt.anchoredPosition.y < -h / 2f - 30f)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, h / 2f + 30f);
                velocities[i] = new Vector2(Random.Range(-40f, 40f), Random.Range(-260f, -120f));
            }
        }
    }
}
