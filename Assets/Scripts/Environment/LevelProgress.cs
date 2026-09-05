using UnityEngine;

/// <summary>
/// Racuna koliki je dio razine Slavko presao i predaje to HUD-u.
///
/// Postoji zbog jednostavnog razloga: u trkacu koji se sam pomice igrac nema
/// nikakav osjecaj koliko je staze ostalo, pa mu kraj razine dode neocekivano.
/// Traka napretka na vrhu ekrana i ciljna vrata na kraju staze zajedno cine
/// zavrsetak razine najavljenim umjesto iznenadnim.
/// </summary>
public class LevelProgress : MonoBehaviour
{
    [Tooltip("Ostavi prazno za automatsko pronalazenje po tagu 'Player'")]
    public Transform player;

    [Tooltip("Polozaj na kojem razina pocinje")]
    public float startX;

    [Tooltip("Polozaj cilja")]
    public float finishX = 400f;

    private void Awake()
    {
        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float span = finishX - startX;
        if (span <= 0f) return;

        float t = Mathf.Clamp01((player.position.x - startX) / span);
        UIManager.Instance?.SetLevelProgress(t);
    }
}
