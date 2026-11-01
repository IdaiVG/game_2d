using System.Collections;
using UnityEngine;

// Vida extra que puede soltar una caja rompible.
// Solo el jugador puede recogerla. Si ya tiene el maximo de vidas el
// coleccionable NO se consume: queda en el escenario para no desperdiciarlo.
[RequireComponent(typeof(Collider2D))]
public class ExtraLifeCollectible : MonoBehaviour
{
    [Header("Recompensa")]
    [Min(1)] public int livesToGrant = 1;

    [Header("Animacion")]
    public bool floatAnimation = true;
    public float floatAmplitude = 0.15f;
    public float floatSpeed = 2.2f;
    public float spinSpeed = 0f;

    [Header("Duracion")]
    [Tooltip("0 = permanece en el escenario hasta que el jugador la recoja.")]
    public float lifetime = 0f;

    private Collider2D lifeCollider;
    private SpriteRenderer lifeRenderer;
    private Vector3 basePosition;
    private float phase;
    private bool collected;
    private float nextFullNoticeTime;

    void Awake()
    {
        lifeCollider = GetComponent<Collider2D>();
        lifeRenderer = GetComponent<SpriteRenderer>();

        if (lifeCollider != null)
            lifeCollider.isTrigger = true;

        basePosition = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        if (lifetime > 0f)
            StartCoroutine(ExpireAfter(lifetime));
    }

    void Update()
    {
        if (collected || !floatAnimation)
            return;

        float offset = Mathf.Sin(Time.time * floatSpeed + phase) * floatAmplitude;

        transform.position = basePosition + new Vector3(0f, offset, 0f);

        if (Mathf.Abs(spinSpeed) > 0.01f)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null || player.IsDead || player.IsLevelCompleted)
            return;

        TryGiveLife();
    }

    void TryGiveLife()
    {
        LivesManager lives = LivesManager.Instance;

        // Sin administrador de vidas no se consume la recompensa
        if (lives == null)
            return;

        if (LivesManager.CurrentLives >= lives.maxLives)
        {
            // Tope de tres vidas: el coleccionable sigue disponible
            if (Time.time >= nextFullNoticeTime)
            {
                nextFullNoticeTime = Time.time + 1.5f;

                BoneHudUI.ShowPopup(
                    transform.position + Vector3.up * 0.6f,
                    "VIDAS AL MÁXIMO",
                    new Color(0.85f, 0.93f, 1f, 1f));
            }

            return;
        }

        bool granted = false;

        for (int i = 0; i < Mathf.Max(1, livesToGrant); i++)
        {
            if (lives.AddLife())
                granted = true;
        }

        if (!granted)
            return;

        collected = true;

        if (lifeCollider != null)
            lifeCollider.enabled = false;

        BoneHudUI.ShowPopup(
            transform.position + Vector3.up * 0.6f,
            "+" + livesToGrant + " VIDA",
            new Color(1f, 0.55f, 0.6f, 1f));

        StartCoroutine(CollectSequence());
    }

    IEnumerator CollectSequence()
    {
        float duration = 0.22f;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Color startColor = lifeRenderer != null ? lifeRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(startScale, startScale * 1.45f, t);

            if (lifeRenderer != null)
            {
                Color c = startColor;
                c.a = startColor.a * (1f - t);
                lifeRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator ExpireAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (collected)
            yield break;

        collected = true;

        if (lifeCollider != null)
            lifeCollider.enabled = false;

        float duration = 0.3f;
        float elapsed = 0f;

        Color startColor = lifeRenderer != null ? lifeRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (lifeRenderer != null)
            {
                Color c = startColor;
                c.a = startColor.a * (1f - Mathf.Clamp01(elapsed / duration));
                lifeRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
